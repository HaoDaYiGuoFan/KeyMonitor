using System.Runtime.InteropServices;
using System.Threading;
using KeyClickCounter.Models;

namespace KeyClickCounter.Services;

/// <summary>
/// Win32 全局低级钩子（WH_KEYBOARD_LL / WH_MOUSE_LL），捕获全系统键盘与鼠标事件。
/// 按键事件通过事件分发；滚轮与移动距离为高频数据，仅在线程内累计，
/// 由 UI 通过 WheelNotches / DistancePixels 轮询读取。
/// </summary>
public sealed class HookService : IDisposable
{
    private const int WH_KEYBOARD_LL = 13;
    private const int WH_MOUSE_LL = 14;

    private const int WM_KEYDOWN = 0x0100;
    private const int WM_KEYUP = 0x0101;
    private const int WM_SYSKEYDOWN = 0x0104;
    private const int WM_SYSKEYUP = 0x0105;

    private const int WM_MOUSEMOVE = 0x0200;
    private const int WM_LBUTTONDOWN = 0x0201;
    private const int WM_RBUTTONDOWN = 0x0204;
    private const int WM_MBUTTONDOWN = 0x0207;
    private const int WM_MOUSEWHEEL = 0x020A;
    private const int WM_XBUTTONDOWN = 0x020B;
    private const int WM_MOUSEHWHEEL = 0x020E;

    private const uint LLKHF_EXTENDED = 0x01;
    private const uint VK_RETURN = 0x0D;

    private const int WheelDelta = 120;

    [StructLayout(LayoutKind.Sequential)]
    private struct KBDLLHOOKSTRUCT
    {
        public uint vkCode;
        public uint scanCode;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MSLLHOOKSTRUCT
    {
        public POINT pt;
        public uint mouseData;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    private delegate IntPtr LowLevelHookProc(int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelHookProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll")]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr GetModuleHandle(string? lpModuleName);

    /// <summary>键盘按键首次按下（长按重复不触发），参数为界面按键 Id。</summary>
    public event Action<string>? KeyPressed;

    /// <summary>鼠标键按下，参数为界面鼠标键 Id。</summary>
    public event Action<string>? MousePressed;

    /// <summary>未映射的罕见按键（媒体键等），仅累计数量。</summary>
    public event Action? OtherKeyPressed;

    private IntPtr _keyboardHook = IntPtr.Zero;
    private IntPtr _mouseHook = IntPtr.Zero;

    // 保存委托引用，防止被 GC 回收后原生回调变成野指针
    private readonly LowLevelHookProc _keyboardProc;
    private readonly LowLevelHookProc _mouseProc;

    // 当前物理按下的键集合：按住不放产生的重复 WM_KEYDOWN 只计第一次
    private readonly HashSet<uint> _downKeys = new();

    // 滚轮 |delta| 累计（钩子线程写，UI 线程读）；格数 = 累计 / WHEEL_DELTA
    private long _wheelDeltaAbs;

    // 光标移动像素累计（钩子线程写，UI 线程读）
    private long _distancePixels;

    // 上一采样点，仅钩子线程访问
    private int _lastX;
    private int _lastY;
    private bool _hasLastPoint;

    public HookService()
    {
        _keyboardProc = KeyboardHookProc;
        _mouseProc = MouseHookProc;
    }

    /// <summary>滚轮滚动格数（上下、左右滚均累计，绝对值口径）。</summary>
    public long WheelNotches => Interlocked.Read(ref _wheelDeltaAbs) / WheelDelta;

    /// <summary>光标累计移动距离（屏幕像素）。</summary>
    public long DistancePixels => Interlocked.Read(ref _distancePixels);

    public bool Start()
    {
        var hMod = GetModuleHandle(null);
        _keyboardHook = SetWindowsHookEx(WH_KEYBOARD_LL, _keyboardProc, hMod, 0);
        _mouseHook = SetWindowsHookEx(WH_MOUSE_LL, _mouseProc, hMod, 0);
        return _keyboardHook != IntPtr.Zero && _mouseHook != IntPtr.Zero;
    }

    private IntPtr KeyboardHookProc(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            int msg = wParam.ToInt32();
            var kbd = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);

            if (msg == WM_KEYDOWN || msg == WM_SYSKEYDOWN)
            {
                if (_downKeys.Add(kbd.vkCode))
                {
                    string? id = ResolveKeyId(kbd);
                    if (id is not null) KeyPressed?.Invoke(id);
                    else OtherKeyPressed?.Invoke();
                }
            }
            else if (msg == WM_KEYUP || msg == WM_SYSKEYUP)
            {
                _downKeys.Remove(kbd.vkCode);
            }
        }
        return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
    }

    private IntPtr MouseHookProc(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            int msg = wParam.ToInt32();
            switch (msg)
            {
                case WM_MOUSEMOVE:
                    OnMouseMove(lParam);
                    break;
                case WM_MOUSEWHEEL:
                case WM_MOUSEHWHEEL:
                    OnWheel(lParam);
                    break;
                default:
                    string? id = msg switch
                    {
                        WM_LBUTTONDOWN => "MouseLeft",
                        WM_RBUTTONDOWN => "MouseRight",
                        WM_MBUTTONDOWN => "MouseMiddle",
                        WM_XBUTTONDOWN => ResolveXButton(lParam),
                        _ => null
                    };
                    if (id is not null) MousePressed?.Invoke(id);
                    break;
            }
        }
        return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
    }

    private void OnMouseMove(IntPtr lParam)
    {
        var ms = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
        if (_hasLastPoint)
        {
            int dx = ms.pt.X - _lastX;
            int dy = ms.pt.Y - _lastY;
            if (dx != 0 || dy != 0)
            {
                long seg = (long)Math.Round(Math.Sqrt((double)dx * dx + (double)dy * dy));
                if (seg > 0) Interlocked.Add(ref _distancePixels, seg);
            }
        }
        _lastX = ms.pt.X;
        _lastY = ms.pt.Y;
        _hasLastPoint = true;
    }

    private void OnWheel(IntPtr lParam)
    {
        var ms = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
        // mouseData 高 16 位为滚轮 delta，有符号（120 = 1 格；高精度滚轮单次可能小于 120）
        int delta = unchecked((short)((ms.mouseData >> 16) & 0xFFFF));
        if (delta != 0) Interlocked.Add(ref _wheelDeltaAbs, Math.Abs(delta));
    }

    private static string? ResolveKeyId(KBDLLHOOKSTRUCT kbd)
    {
        // 扩展键回车 = 小键盘 Enter（主 Enter 与其共用 VK_RETURN，需按扩展位区分）
        if (kbd.vkCode == VK_RETURN && (kbd.flags & LLKHF_EXTENDED) != 0)
            return "NumEnter";
        return KeyMapping.GetId(kbd.vkCode);
    }

    private static string ResolveXButton(IntPtr lParam)
    {
        var ms = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
        // mouseData 高 16 位：1 = XBUTTON1，2 = XBUTTON2
        uint hi = (ms.mouseData >> 16) & 0xFFFF;
        return hi == 2 ? "MouseX2" : "MouseX1";
    }

    /// <summary>清零会话内滚轮 / 距离累计（重置统计用）。</summary>
    public void ResetLiveCounters()
    {
        Interlocked.Exchange(ref _wheelDeltaAbs, 0);
        Interlocked.Exchange(ref _distancePixels, 0);
        _hasLastPoint = false;
    }

    public void Dispose()
    {
        if (_keyboardHook != IntPtr.Zero)
        {
            UnhookWindowsHookEx(_keyboardHook);
            _keyboardHook = IntPtr.Zero;
        }
        if (_mouseHook != IntPtr.Zero)
        {
            UnhookWindowsHookEx(_mouseHook);
            _mouseHook = IntPtr.Zero;
        }
    }
}