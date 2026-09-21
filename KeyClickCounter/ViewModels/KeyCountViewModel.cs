using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;
using KeyClickCounter.Models;
using KeyClickCounter.Services;

namespace KeyClickCounter.ViewModels;

/// <summary>MVVM 核心：维护每个按键 / 鼠标键的计数与高亮，聚合总计、滚轮与移动距离。</summary>
public class KeyCountViewModel : INotifyPropertyChanged
{
    private const int NotchesPerRevolution = 24; // 常见滚轮每圈格数，1 圈 = 24 格

    private readonly DateTime _startTime = DateTime.Now;
    private readonly Dictionary<string, DateTime> _flashUntil = new();
    private readonly DispatcherTimer _flashTimer;
    private readonly DispatcherTimer _clockTimer;
    private readonly KeyItem _wheelItem;
    private HookService? _hook;
    private long _lastWheelNotches = -1;
    private double _pxPerCm;
    private long _otherCount;        // 当前筛选范围内的“其他键”计数
    private long _staticWheel;       // 筛选范围内已落盘滚轮格数（含已提交增量）
    private long _staticDistance;    // 筛选范围内已落盘光标移动像素（含已提交增量）
    private long _committedLiveWheel;    // 已并入分桶的实时滚轮格数（防止重复累计）
    private long _committedLiveDistance; // 已并入分桶的实时移动像素（防止重复累计）
    private bool _isTopmost;
    private string _statusText = "";
    private string _liveStatsText = "";

    // ---- 日期维度统计 ----
    private DateTime _startDate = DateTime.Today;                              // 首次启动统计日期
    private DateTime _currentDay = DateTime.Today;                             // 会话累计所属自然日（跨天切换）
    private readonly Dictionary<DateTime, Dictionary<string, long>> _days = new(); // 日期 -> 各键计数
    private readonly Dictionary<string, long> _session = new();               // 今日未落盘按键增量
    private long _sessionOther;                                               // 今日未落盘其他键增量
    private DateTime _rangeStart = DateTime.Today;                            // 筛选起始日期
    private DateTime _rangeEnd = DateTime.Today;                              // 筛选结束日期
    private bool _rangeEndTracksToday = true;                                 // 结束日期是否跟随“今天”自动延伸

    public List<KeyItem> Keys { get; }

    public List<KeyItem> MouseKeys { get; }

    public Dictionary<string, KeyItem> Map { get; }

    public double KeyboardWidth => KeyboardLayout.CanvasWidth;

    public double KeyboardHeight => KeyboardLayout.CanvasHeight;

    public ICommand ResetCommand { get; }

    public ICommand DefaultRangeCommand { get; }

    public KeyCountViewModel()
    {
        Keys = KeyboardLayout.BuildKeyboardKeys();
        MouseKeys = KeyboardLayout.BuildMouseKeys();
        Map = Keys.Concat(MouseKeys).ToDictionary(k => k.Id, k => k);
        _wheelItem = Map["MouseWheel"];

        // 默认像素/厘米换算系数 = 系统 DPI / 25.4mm；工具栏可手动校准
        using (var g = System.Drawing.Graphics.FromHwnd(IntPtr.Zero))
        {
            _pxPerCm = Math.Round(g.DpiX / 2.54, 1);
        }
        if (_pxPerCm < 1) _pxPerCm = 37.8;

        ResetCommand = new RelayCommand(_ => ResetAll());
        DefaultRangeCommand = new RelayCommand(_ => ResetRangeToDefault());

        // 高亮 200ms 自动熄灭
        _flashTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(33) };
        _flashTimer.Tick += (_, _) => SweepFlashes();
        _flashTimer.Start();

        // 每秒刷新：运行时长、滚轮、移动距离（高频数据轮询，不走事件）
        _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _clockTimer.Tick += (_, _) => UpdateLiveStats();
        _clockTimer.Start();

        EnsureTodayBucket();
        RecomputeDisplayed();
    }

    private bool _isDarkTheme = true;

    /// <summary>当前是否深色皮肤；切换即触发 ThemeChanged。</summary>
    public bool IsDarkTheme
    {
        get => _isDarkTheme;
        set
        {
            if (_isDarkTheme == value) return;
            _isDarkTheme = value;
            OnPropertyChanged(nameof(IsDarkTheme));
            ThemeChanged?.Invoke();
        }
    }

    /// <summary>启动时装载主题，不触发重绘事件。</summary>
    public void SetThemeSilent(bool isDark)
    {
        _isDarkTheme = isDark;
        OnPropertyChanged(nameof(IsDarkTheme));
    }

    public event Action? ThemeChanged;

    public bool IsTopmost
    {
        get => _isTopmost;
        set
        {
            if (_isTopmost == value) return;
            _isTopmost = value;
            OnPropertyChanged(nameof(IsTopmost));
        }
    }

    /// <summary>px/cm 换算系数（可校准）。</summary>
    public double PxPerCm
    {
        get => _pxPerCm;
        set
        {
            double v = value < 1 ? 1 : value;
            if (_pxPerCm == v) return;
            _pxPerCm = v;
            OnPropertyChanged(nameof(PxPerCm));
            OnPropertyChanged(nameof(DistanceText));
            RefreshLiveStatsText();
        }
    }

    /// <summary>当前筛选范围内滚轮格数（含今天的未落盘实时增量）。</summary>
    public long TotalWheelNotches => _staticWheel + (TodayInRange ? ((_hook?.WheelNotches ?? 0) - _committedLiveWheel) : 0);

    /// <summary>当前筛选范围内光标移动像素（含今天的未落盘实时增量）。</summary>
    public long TotalDistancePixels => _staticDistance + (TodayInRange ? ((_hook?.DistancePixels ?? 0) - _committedLiveDistance) : 0);

    public string WheelRotationsText => (TotalWheelNotches / (double)NotchesPerRevolution).ToString("0.#");

    public string DistanceText
    {
        get
        {
            double cm = TotalDistancePixels / _pxPerCm;
            return cm < 100 ? cm.ToString("0.#") + " cm" : (cm / 100).ToString("0.##") + " m";
        }
    }

    public string LiveStatsText => _liveStatsText;

    public long OtherCount => _otherCount;

    public long TotalKeys => Keys.Sum(k => k.Count);

    public long TotalMouse => MouseKeys.Where(k => k.Id != "MouseWheel").Sum(k => k.Count);

    public long Total => TotalKeys + TotalMouse;

    public string RunTimeText => (DateTime.Now - _startTime).ToString(@"hh\:mm\:ss");

    public string StatusText => _statusText;

    /// <summary>筛选起始日期（默认 = 首次启动统计日期）。</summary>
    public DateTime RangeStart
    {
        get => _rangeStart;
        set
        {
            var v = value.Date;
            if (v < _startDate) v = _startDate;
            if (v > _rangeEnd) v = _rangeEnd;
            if (_rangeStart == v)
            {
                // 选择日期被钳制回当前值（如首次启动当天：任何起始日期都会被收窄到今天）：
                // 仍需通知一次，让 DatePicker 回显实际生效的日期，避免选择器显示的值
                // 与标题栏 / 状态栏显示的“统计范围”不一致。
                if (value.Date != v) OnPropertyChanged(nameof(RangeStart));
                return;
            }
            _rangeStart = v;
            OnPropertyChanged(nameof(RangeStart));
            RecomputeDisplayed();
        }
    }

    /// <summary>筛选结束日期（默认 = 今天；“跟随今天”时跨天自动延伸到新的一天）。</summary>
    public DateTime RangeEnd
    {
        get => _rangeEnd;
        set
        {
            var v = value.Date;
            if (v > DateTime.Today) v = DateTime.Today;
            if (v < _rangeStart) v = _rangeStart;
            _rangeEndTracksToday = v == DateTime.Today;
            if (_rangeEnd == v)
            {
                // 与 RangeStart 同理：被钳制回当前值时通知一次，让选择器与实际生效日期同步。
                if (value.Date != v) OnPropertyChanged(nameof(RangeEnd));
                return;
            }
            _rangeEnd = v;
            OnPropertyChanged(nameof(RangeEnd));
            RecomputeDisplayed();
        }
    }

    /// <summary>挂接钩子服务后，滚轮 / 距离才有会话数据。</summary>
    public void SetHookService(HookService hook)
    {
        _hook = hook;
        UpdateLiveStats(force: true);
    }

    /// <summary>
    /// 装载持久化统计（首次启动日期 + 按天分桶），初始化筛选范围并重建卡片计数。
    /// 旧版扁平数据由 StorageService.Load 已迁移到“今天”分桶。
    /// </summary>
    public void InitializeFromStorage(StorageData data)
    {
        if (DateTime.TryParse(data.StartDate, out var sd)) _startDate = sd.Date;
        if (_startDate > DateTime.Today) _startDate = DateTime.Today;
        foreach (var day in data.Days)
        {
            if (DateTime.TryParse(day.Key, out var d))
                _days[d.Date] = new Dictionary<string, long>(day.Value);
        }
        _currentDay = DateTime.Today;
        _committedLiveWheel = 0;
        _committedLiveDistance = 0;
        EnsureTodayBucket();
        ResetRangeToDefault();
        RecomputeDisplayed();
    }

    /// <summary>恢复默认筛选范围：首次启动统计日期 ~ 今天（结束日期跟随今天自动延伸）。</summary>
    public void ResetRangeToDefault()
    {
        _rangeStart = _startDate;
        _rangeEnd = DateTime.Today;
        _rangeEndTracksToday = true;
        OnPropertyChanged(nameof(RangeStart));
        OnPropertyChanged(nameof(RangeEnd));
        RecomputeDisplayed();
    }

    /// <summary>根据当前日期分桶重建卡片显示与总计（UI 线程调用）。</summary>
    public void RecomputeDisplayed()
    {
        RecomputeStaticSums();
        var bucket = DayBucket(_rangeStart);
        foreach (var k in Map.Values)
        {
            k.Count = bucket.TryGetValue(k.Id, out var c) ? c : 0;
            if (TodayInRange && _session.TryGetValue(k.Id, out var s)) k.Count += s;
        }
        _otherCount = (TodayInRange ? _sessionOther : 0) + DaysOther(_rangeStart, _rangeEnd);
        _lastWheelNotches = TotalWheelNotches;
        _wheelItem.Count = _lastWheelNotches;
        OnPropertyChanged(nameof(DistanceText));
        OnPropertyChanged(nameof(WheelRotationsText));
        RefreshLiveStatsText();
        RaiseTotals();
    }

    /// <summary>钩子线程经 Dispatcher 调度后在 UI 线程调用：记录一次按键。</summary>
    public void RegisterKeyPress(string id)
    {
        if (!Map.TryGetValue(id, out var item)) return;
        _session[id] = _session.GetValueOrDefault(id) + 1;
        item.Count++;
        item.IsFlash = true;
        _flashUntil[id] = DateTime.UtcNow.AddMilliseconds(200);
        RaiseTotals();
    }

    public void RegisterOtherKey()
    {
        _sessionOther++;
        _otherCount++;
        RaiseTotals();
    }

    /// <summary>重置统计：清空当前筛选范围内的数据（含滚轮 / 移动距离），未落盘增量一并丢弃。</summary>
    public void ResetAll()
    {
        for (var d = _rangeStart; d <= _rangeEnd; d = d.AddDays(1)) _days.Remove(d);
        if (TodayInRange)
        {
            _hook?.ResetLiveCounters();
            _session.Clear();
            _sessionOther = 0;
            _committedLiveWheel = 0;
            _committedLiveDistance = 0;
        }
        RecomputeDisplayed();
    }

    /// <summary>把今日未落盘增量并入今日分桶后一次性落盘（防止强杀进程丢失最后一次计数）。</summary>
    public StorageData BuildStorageData()
    {
        CommitSession(DateTime.Today);
        RecomputeDisplayed();
        return new StorageData
        {
            Version = 2,
            StartDate = _startDate.ToString("yyyy-MM-dd"),
            ThemeIsLight = _isDarkTheme ? 0 : 1,
            Days = _days.ToDictionary(
                kv => kv.Key.ToString("yyyy-MM-dd"),
                kv => new Dictionary<string, long>(kv.Value))
        };
    }

    // ---- 日期维度统计私有辅助 ----

    /// <summary>跨天时：把前一日的未落盘增量并入其分桶，并把会话累计基准切到新的一天。</summary>
    private void CheckDayRollover()
    {
        var today = DateTime.Today;
        if (today == _currentDay) return;
        CommitSession(_currentDay);
        _currentDay = today;
        EnsureTodayBucket();
        _hook?.ResetLiveCounters();
        _committedLiveWheel = 0;
        _committedLiveDistance = 0;
        if (_rangeEndTracksToday)
        {
            _rangeEnd = today;
            OnPropertyChanged(nameof(RangeEnd));
        }
        RecomputeDisplayed();
    }

    private bool TodayInRange => _rangeStart <= DateTime.Today && DateTime.Today <= _rangeEnd;

    /// <summary>确保今日分桶存在。</summary>
    private void EnsureTodayBucket()
    {
        if (!_days.ContainsKey(DateTime.Today)) _days[DateTime.Today] = new Dictionary<string, long>();
    }

    /// <summary>某天的分桶（不存在时返回空桶）。</summary>
    private Dictionary<string, long> DayBucket(DateTime day)
        => _days.TryGetValue(day.Date, out var b) ? b : new Dictionary<string, long>();

    /// <summary>把某日的未落盘增量并入该日分桶（按增量累计，避免重复计数）。</summary>
    private void CommitSession(DateTime day)
    {
        if (!_days.TryGetValue(day, out var bucket)) bucket = _days[day] = new Dictionary<string, long>();
        long wheel = _hook?.WheelNotches ?? 0;
        long dist = _hook?.DistancePixels ?? 0;
        long wDelta = wheel - _committedLiveWheel;
        long dDelta = dist - _committedLiveDistance;
        if (wDelta > 0) bucket["WheelNotches"] = bucket.GetValueOrDefault("WheelNotches") + wDelta;
        if (dDelta > 0) bucket["CursorDistancePx"] = bucket.GetValueOrDefault("CursorDistancePx") + dDelta;
        foreach (var kv in _session) bucket[kv.Key] = bucket.GetValueOrDefault(kv.Key) + kv.Value;
        if (_sessionOther > 0)
        {
            bucket["OtherCount"] = bucket.GetValueOrDefault("OtherCount") + _sessionOther;
            _sessionOther = 0;
        }
        _session.Clear();
        _committedLiveWheel = wheel;
        _committedLiveDistance = dist;
    }

    /// <summary>区间内“其他键”历史计数（按天叠加）。</summary>
    private long DaysOther(DateTime from, DateTime to)
    {
        long sum = 0;
        for (var d = from; d <= to; d = d.AddDays(1))
        {
            if (_days.TryGetValue(d, out var b) && b.TryGetValue("OtherCount", out var c)) sum += c;
        }
        return sum;
    }

    /// <summary>按当前筛选范围重建滚轮 / 移动距离静态基线（已落盘部分）。</summary>
    private void RecomputeStaticSums()
    {
        _staticWheel = 0;
        _staticDistance = 0;
        for (var d = _rangeStart; d <= _rangeEnd; d = d.AddDays(1))
        {
            if (!_days.TryGetValue(d, out var b)) continue;
            if (b.TryGetValue("WheelNotches", out var w)) _staticWheel += w;
            if (b.TryGetValue("CursorDistancePx", out var c)) _staticDistance += c;
        }
    }

    private void UpdateLiveStats(bool force = false)
    {
        CheckDayRollover();
        if (_hook is null) return;
        long notches = TotalWheelNotches;
        if (force || notches != _lastWheelNotches)
        {
            if (notches != _wheelItem.Count)
            {
                _wheelItem.Count = notches;
                _wheelItem.IsFlash = true;
                _flashUntil[_wheelItem.Id] = DateTime.UtcNow.AddMilliseconds(200);
            }
            _lastWheelNotches = notches;
            OnPropertyChanged(nameof(WheelRotationsText));
        }
        OnPropertyChanged(nameof(DistanceText));
        RefreshLiveStatsText();
        RaiseTotals();
    }

    private void RefreshLiveStatsText()
    {
        _liveStatsText = $"滚轮 {TotalWheelNotches:N0} 格 ≈ {WheelRotationsText} 圈（{NotchesPerRevolution} 格/圈）"
                       + $" · 光标移动 {DistanceText}（{TotalDistancePixels:N0} px）";
        OnPropertyChanged(nameof(LiveStatsText));
    }

    private void SweepFlashes()
    {
        if (_flashUntil.Count == 0) return;
        var now = DateTime.UtcNow;
        List<string>? expired = null;
        foreach (var kvp in _flashUntil)
        {
            if (kvp.Value <= now) (expired ??= new List<string>()).Add(kvp.Key);
        }
        if (expired is null) return;
        foreach (var id in expired)
        {
            _flashUntil.Remove(id);
            if (Map.TryGetValue(id, out var item)) item.IsFlash = false;
        }
    }

    private void RaiseTotals()
    {
        _statusText = $"统计范围 {_rangeStart:yyyy-MM-dd} ~ {_rangeEnd:yyyy-MM-dd}"
                      + $" · 总按键 {TotalKeys} · 总鼠标 {TotalMouse} · 合计 {Total}"
                      + $" · 滚轮 {TotalWheelNotches:N0} 格 · 移动 {DistanceText} · 运行 {RunTimeText}"
                      + (_otherCount > 0 ? $" · 其他键 {_otherCount}" : "");
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(TotalKeys));
        OnPropertyChanged(nameof(TotalMouse));
        OnPropertyChanged(nameof(Total));
        OnPropertyChanged(nameof(RunTimeText));
        OnPropertyChanged(nameof(OtherCount));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string name)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}