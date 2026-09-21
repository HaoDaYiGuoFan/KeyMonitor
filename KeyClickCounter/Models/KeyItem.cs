using System.ComponentModel;

namespace KeyClickCounter.Models;

/// <summary>单个按键 / 鼠标键的 UI 模型：名称、键码、计数与高亮状态。</summary>
public class KeyItem : INotifyPropertyChanged
{
    private long _count;
    private bool _isFlash;

    public KeyItem(string id, string displayName, uint virtualKey, double x, double y,
                   double cellWidth, double cellHeight, string? subLabel = null,
                   bool isMouse = false, bool isFn = false)
    {
        Id = id;
        DisplayName = displayName;
        VirtualKey = virtualKey;
        X = x;
        Y = y;
        CellWidth = cellWidth;
        CellHeight = cellHeight;
        SubLabel = subLabel;
        IsMouse = isMouse;
        IsFn = isFn;
    }

    /// <summary>稳定标识，同时作为持久化 JSON 的键。</summary>
    public string Id { get; }

    public string DisplayName { get; }

    /// <summary>键帽上的第二标识（上档符号 / 小键盘功能名），可为空。</summary>
    public string? SubLabel { get; }

    /// <summary>Win32 虚拟键码；鼠标键与 Fn 等不可捕获键为 0。</summary>
    public uint VirtualKey { get; }

    public bool IsMouse { get; }

    public bool IsFn { get; }

    /// <summary>画布绝对坐标（已含间距，像素）。</summary>
    public double X { get; }

    public double Y { get; }

    public double CellWidth { get; }

    public double CellHeight { get; }

    public bool HasSubLabel => !string.IsNullOrEmpty(SubLabel);

    public double NameFontSize => IsMouse ? 13 : 11;

    public long Count
    {
        get => _count;
        set
        {
            if (_count == value) return;
            _count = value;
            OnPropertyChanged(nameof(Count));
            OnPropertyChanged(nameof(CountText));
        }
    }

    public string CountText => FormatCount(_count);

    public static string FormatCount(long c)
    {
        if (c < 0) c = 0;
        if (c < 100000) return c.ToString();
        if (c < 10000000) return (c / 1000d).ToString("0.#") + "k";
        return (c / 1000000d).ToString("0.#") + "M";
    }

    public bool IsFlash
    {
        get => _isFlash;
        set
        {
            if (_isFlash == value) return;
            _isFlash = value;
            OnPropertyChanged(nameof(IsFlash));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string name)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}