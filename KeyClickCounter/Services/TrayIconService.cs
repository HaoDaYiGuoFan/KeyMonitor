using System.Drawing;
using System.Windows.Forms;

namespace KeyClickCounter.Services;

/// <summary>系统托盘图标与右键菜单（WinForms NotifyIcon）。</summary>
public sealed class TrayIconService : IDisposable
{
    private readonly NotifyIcon _notifyIcon;
    private readonly ToolStripMenuItem ShowItem;
    private readonly ToolStripMenuItem ResetItem;
    private readonly ToolStripMenuItem ExitItem;

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
        ShowItem = new ToolStripMenuItem(LocalizationService.GetText("TrayShow"), null, (_, _) => showWindow());
        ResetItem = new ToolStripMenuItem(LocalizationService.GetText("TrayReset"), null, (_, _) => resetStats());
        menu.Items.Add(ShowItem);
        menu.Items.Add(ResetItem);
        menu.Items.Add(new ToolStripSeparator());
        ExitItem = new ToolStripMenuItem(LocalizationService.GetText("TrayExit"), null, (_, _) => ExitRequested?.Invoke());
        menu.Items.Add(ExitItem);

        _notifyIcon = new NotifyIcon
        {
            Text = "KeyClickCounter",
            Icon = icon,
            Visible = true,
            ContextMenuStrip = menu
        };
        _notifyIcon.DoubleClick += (_, _) => showWindow();
    }

    /// <summary>语言切换后刷新托盘菜单文字。</summary>
    public void ApplyLanguage()
    {
        ShowItem.Text = LocalizationService.GetText("TrayShow");
        ResetItem.Text = LocalizationService.GetText("TrayReset");
        ExitItem.Text = LocalizationService.GetText("TrayExit");
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