namespace KeyClickCounter.Models;

/// <summary>
/// 带日期维度的持久化数据：首次启动统计日期 + 按天分桶的每个按键/鼠标键计数。
/// 每天桶内特殊键：WheelNotches（滚轮格数）、CursorDistancePx（光标移动像素）、OtherCount（其他键）。
/// </summary>
public class StorageData
{
    public int Version { get; set; } = 2;

    /// <summary>首次启动程序开始统计的日期（yyyy-MM-dd）。</summary>
    public string StartDate { get; set; } = "";

    /// <summary>按日期（yyyy-MM-dd）分桶的计数。</summary>
    public Dictionary<string, Dictionary<string, long>> Days { get; set; } = new();

    /// <summary>主题：1 = 浅色，0 = 深色（兼容旧字段）。</summary>
    public int? ThemeIsLight { get; set; }

    /// <summary>界面语言偏好（zh-Hans / en）；为空时启动按系统语言自动匹配。</summary>
    public string? Language { get; set; }
}
