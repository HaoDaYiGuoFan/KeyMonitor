﻿using System.Globalization;
using System.Windows;
using Application = System.Windows.Application;

namespace KeyClickCounter.Services;

/// <summary>
/// 界面语言服务：
/// 启动时按系统语言自动匹配界面（中文系统自动用中文，其余自动用英文），
/// 也可在工具栏通过“English / 中文”按钮一键中英切换，切换结果会持久化到 keycount.json。
/// 语言文本全部存放在 App 级资源词典（Resources/Strings.xaml 中文、Resources/Strings.en.xaml 英文），
/// 切换时整体替换对应词典，XAML 中的 DynamicResource 自动跟随刷新；
/// 代码生成的文本（状态栏 / 托盘菜单 / 键帽）通过 GetText 读取，并依赖 LanguageChanged 事件重建。
/// </summary>
public static class LocalizationService
{
    public const string Chinese = "zh-Hans";
    public const string English = "en";

    /// <summary>App.xaml 合并词典中语言词典的固定下标：0=主题，1=语言，2=键盘布局。</summary>
    private const int StringsDictionaryIndex = 1;

    /// <summary>当前语言键（zh-Hans / en）。</summary>
    public static string CurrentKey { get; private set; } = Chinese;

    public static bool IsChinese => CurrentKey == Chinese;

    /// <summary>语言切换后触发（UI 线程），用于重建代码生成的文本（状态栏 / 托盘 / 键帽）。</summary>
    public static event Action? LanguageChanged;

    private static bool _initialized;

    /// <summary>
    /// 启动时调用：优先恢复上次保存的语言，否则按系统语言自动匹配。
    /// 该过程不触发 LanguageChanged，由调用方完成首次界面刷新。
    /// </summary>
    public static void Initialize(string? savedLanguage)
    {
        _initialized = true;
        string target = savedLanguage switch
        {
            Chinese or English => savedLanguage,
            _ => CultureInfo.CurrentUICulture.Name.StartsWith("zh", StringComparison.OrdinalIgnoreCase)
                ? Chinese : English
        };
        if (target != CurrentKey) Apply(target);
    }

    /// <summary>中 ⇄ 英一键切换。</summary>
    public static void Toggle() => SetLanguage(CurrentKey == Chinese ? English : Chinese);

    /// <summary>切到指定语言并广播 LanguageChanged（手动切换走这里）。</summary>
    public static void SetLanguage(string key)
    {
        if (key != Chinese && key != English) return;
        if (!_initialized) Initialize(null);
        if (CurrentKey == key) return;
        Apply(key);
        LanguageChanged?.Invoke();
    }

    /// <summary>从 XAML 词典取本地化文本；资源缺失时回退为 key 本身，便于排查。</summary>
    public static string GetText(string key)
        => Application.Current?.TryFindResource("L." + key) as string ?? key;

    private static void Apply(string key)
    {
        CurrentKey = key;
        var uri = new Uri(key == Chinese ? "Resources/Strings.xaml" : "Resources/Strings.en.xaml", UriKind.Relative);
        Application.Current.Resources.MergedDictionaries[StringsDictionaryIndex] = new ResourceDictionary { Source = uri };
    }
}