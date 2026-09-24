using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using KeyClickCounter.Services;
using KeyClickCounter.ViewModels;
using WinForms = System.Drawing;

namespace KeyClickCounter;

public static class DwmTheme
{
    [DllImport("dwmapi.dll")]
    public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);
}



public partial class MainWindow : Window
{
    private const int WM_ENDSESSION = 0x0016;

    private readonly KeyCountViewModel _viewModel = new();
    private readonly HookService _hookService = new();
    private readonly TrayIconService _trayIcon;
    private readonly DispatcherTimer _autosaveTimer;
    private bool _realExit;

    public MainWindow()
    {
        // 启动时恢复上次退出保存的统计（AppData；自动迁移旧版程序目录数据；含首次启动日期与按天分桶）
        StorageService.MigrateLegacy();
        var saved = StorageService.Load();
        // 多语言：优先恢复上次保存的语言，否则按系统语言自动匹配（中文系统→中文，其余→英文）
        LocalizationService.Initialize(saved.Language);
        _viewModel.InitializeFromStorage(saved);
        // 主题：json 里 ThemeIsLight=1 表示浅色
        _viewModel.SetThemeSilent(!(saved.ThemeIsLight == 1));

        // 键帽与状态栏文本按当前语言初始化
        _viewModel.RefreshKeyNames();

        DataContext = _viewModel;
        InitializeComponent();

        LocalizationService.LanguageChanged += OnLanguageChanged;

        // 钩子回调在钩子线程，统一调度到 UI 线程
        _hookService.KeyPressed += id => Dispatcher.InvokeAsync(() => _viewModel.RegisterKeyPress(id));
        _hookService.MousePressed += id => Dispatcher.InvokeAsync(() => _viewModel.RegisterKeyPress(id));
        _hookService.OtherKeyPressed += () => Dispatcher.InvokeAsync(() => _viewModel.RegisterOtherKey());
        if (!_hookService.Start())
        {
            System.Windows.MessageBox.Show(this, LocalizationService.GetText("HookInstallFailed"),
                "KeyClickCounter", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        _viewModel.SetHookService(_hookService);
        _viewModel.ThemeChanged += ApplyTheme;
        ApplyTheme();

        // 自动保存：每 30 秒落盘一次，强杀进程最多丢 30 秒数据
        _autosaveTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
        _autosaveTimer.Tick += (_, _) => SaveCurrent();
        _autosaveTimer.Start();

        _trayIcon = new TrayIconService(ShowFromTray, _viewModel.ResetAll);
        _trayIcon.ExitRequested += () =>
        {
            _realExit = true;
            Close();
        };
        _viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(KeyCountViewModel.Total)) UpdateTrayTotalText();
        };

        TryLoadWindowIcon();
    }

    /// <summary>语言切换后：刷新键帽 / 状态栏文本、托盘菜单与托盘提示，并立即持久化语言偏好。</summary>
    private void OnLanguageChanged()
    {
        _viewModel.RefreshKeyNames();
        _viewModel.RefreshLanguageTexts();
        _trayIcon?.ApplyLanguage();
        UpdateTrayTotalText();
        SaveCurrent(); // 立即落盘 Language，下次启动沿用该语言
    }

    private void UpdateTrayTotalText()
    {
        _trayIcon?.UpdateText(string.Format(LocalizationService.GetText("TrayTooltipFmt"), _viewModel.Total));
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        ApplyTheme(); // SourceInitialized 后再应用一次，确保标题栏首绘即正确
        var source = (HwndSource)PresentationSource.FromVisual(this);
        source.AddHook(WndProc);
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_ENDSESSION && wParam != IntPtr.Zero)
        {
            // Windows 关机 / 注销 / 重启：走真实退出路径（保存计数 + 释放钩子），不阻塞系统
            _realExit = true;
            Close();
            handled = true;
        }
        return IntPtr.Zero;
    }

    /// <summary>从托盘恢复主窗口（托盘双击/菜单、第二实例唤醒共用）。</summary>
    public void ShowFromTray()
    {
        Show();
        if (WindowState == WindowState.Minimized) WindowState = WindowState.Normal;
        Activate();
    }

    private void HideToTray_Click(object sender, RoutedEventArgs e)
    {
        SaveCurrent();
        Hide();
    }

    protected override void OnStateChanged(EventArgs e)
    {
        base.OnStateChanged(e);
        if (WindowState == WindowState.Minimized)
        {
            SaveCurrent();
            Hide();
        }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        // 关闭按钮 = 缩到托盘；仅托盘菜单"退出程序"或系统关机才真正退出
        if (!_realExit)
        {
            SaveCurrent();
            e.Cancel = true;
            Hide();
            return;
        }
        base.OnClosing(e);
    }

    protected override void OnClosed(EventArgs e)
    {
        try
        {
            _autosaveTimer.Stop();
            SaveCurrent();
        }
        finally
        {
            _hookService.Dispose();
            _trayIcon.Dispose();
            base.OnClosed(e);
        }
    }

    /// <summary>替换合并字典首位实现皮肤切换，DynamicResource 自动跟随。</summary>
    private void ApplyTheme()
    {
        var uri = new Uri(_viewModel.IsDarkTheme ? "Themes/Dark.xaml" : "Themes/Light.xaml", UriKind.Relative);
        System.Windows.Application.Current.Resources.MergedDictionaries[0] = new ResourceDictionary { Source = uri };
        // Win32 标题栏跟随主题（DWMWA_USE_IMMERSIVE_DARK_MODE=20）
        if (System.Windows.Application.Current.MainWindow is System.Windows.Window w && w.IsLoaded)
        {
            int dark = _viewModel.IsDarkTheme ? 1 : 0;
            DwmTheme.DwmSetWindowAttribute(new System.Windows.Interop.WindowInteropHelper(w).Handle, 20, ref dark, sizeof(int));
        }
    }
    /// <summary>当前计数 + 滚轮 / 距离增量合并进当天分桶后一次性落盘。</summary>
    private void SaveCurrent()
    {
        StorageService.Save(_viewModel.BuildStorageData());
    }

    private void TryLoadWindowIcon()
    {
        try
        {
            if (Environment.ProcessPath is { } exe &&
                WinForms.Icon.ExtractAssociatedIcon(exe) is { } icon)
            {
                Icon = Imaging.CreateBitmapSourceFromHIcon(icon.Handle,
                    Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
        }
        catch { /* 无图标不影响运行 */ }
    }
}