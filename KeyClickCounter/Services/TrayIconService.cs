using System.Drawing;
using System.Windows.Forms;

namespace KeyClickCounter.Services;

/// <summary>系统托盘图标与右键菜单（WinForms NotifyIcon）。</summary>
public sealed class TrayIconService : IDisposable
{
    private readonly NotifyIcon _notifyIcon;

    /// <summary>用户点击“退出程序”。</summary>
    public event Action? ExitRequested;

    public TrayIconService(Action showWindow, Action resetStats)
    {
        Icon? icon = null;
        try
        {
            if (Environment.ProcessPath is { } exe) icon = Icon.ExtractAssociatedIcon(exe);
        }
        catch { /* 图标缺失不影响功能 */ }

        var menu = new ContextMenuStrip();
        menu.Items.Add("显示窗口", null, (_, _) => showWindow());
        menu.Items.Add("重置统计数据", null, (_, _) => resetStats());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("退出程序", null, (_, _) => ExitRequested?.Invoke());

        _notifyIcon = new NotifyIcon
        {
            Text = "KeyClickCounter",
            Icon = icon,
            Visible = true,
            ContextMenuStrip = menu
        };
        _notifyIcon.DoubleClick += (_, _) => showWindow();
    }

    public void UpdateText(string text)
    {
        try { _notifyIcon.Text = text.Length <= 63 ? text : text[..63]; }
        catch { /* 托盘不可用时忽略 */ }
    }

    public void Dispose()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
    }
}