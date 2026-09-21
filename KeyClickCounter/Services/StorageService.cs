using System.IO;
using System.Text.Json;
using KeyClickCounter.Models;

namespace KeyClickCounter.Services;

/// <summary>
/// JSON 持久化：数据存 %APPDATA%\KeyClickCounter\keycount.json（重装/换目录不丢）。
/// 旧版本存在程序目录的 keycount.json 会在启动时自动迁移。
/// </summary>
public static class StorageService
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static string DirPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "KeyClickCounter");

    public static string FilePath => Path.Combine(DirPath, "keycount.json");

    /// <summary>
    /// 读取统计（含首次启动日期与按天分桶）。旧版扁平格式自动迁移到“今天”分桶。
    /// </summary>
    public static StorageData Load()
    {
        try
        {
            if (!File.Exists(FilePath)) return NewEmpty();
            var text = File.ReadAllText(FilePath);
            using var doc = JsonDocument.Parse(text);
            if (doc.RootElement.ValueKind != JsonValueKind.Object) return NewEmpty();

            if (doc.RootElement.TryGetProperty("Days", out _) || doc.RootElement.TryGetProperty("Version", out _))
            {
                var data = JsonSerializer.Deserialize<StorageData>(text);
                if (data is null) return NewEmpty();
                if (!DateTime.TryParse(data.StartDate, out _))
                    data.StartDate = DateTime.Today.ToString("yyyy-MM-dd");
                return data;
            }

            // 旧版扁平格式：计数并入“今天”分桶（不修改原文件，下次保存自动升级为新格式）
            var today = DateTime.Today.ToString("yyyy-MM-dd");
            var bucket = new Dictionary<string, long>();
            int? theme = null;
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                if (prop.Name == "ThemeIsLight")
                {
                    if (prop.Value.ValueKind == JsonValueKind.Number && prop.Value.TryGetInt32(out int t)) theme = t;
                    continue;
                }
                if (prop.Value.ValueKind != JsonValueKind.Number || !prop.Value.TryGetInt64(out long v) || v <= 0) continue;
                // 旧版滚轮计数的键名是 MouseWheel，新格式统一定义为 WheelNotches
                bucket[prop.Name == "MouseWheel" ? "WheelNotches" : prop.Name] = v;
            }
            return new StorageData
            {
                Version = 2,
                StartDate = today,
                ThemeIsLight = theme,
                Days = new Dictionary<string, Dictionary<string, long>> { [today] = bucket }
            };
        }
        catch
        {
            return NewEmpty();
        }
    }

    private static StorageData NewEmpty()
        => new() { Version = 2, StartDate = DateTime.Today.ToString("yyyy-MM-dd") };

    public static void Save(StorageData data)
    {
        try
        {
            Directory.CreateDirectory(DirPath);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(data, Options));
        }
        catch
        {
            // 保存失败不阻塞退出流程
        }
    }

    /// <summary>旧版本把数据存在程序目录；首次运行时迁移到 AppData（复制，不动原文件）。</summary>
    public static void MigrateLegacy()
    {
        try
        {
            if (File.Exists(FilePath)) return;
            string legacy = Path.Combine(AppContext.BaseDirectory, "keycount.json");
            if (File.Exists(legacy))
            {
                Directory.CreateDirectory(DirPath);
                File.Copy(legacy, FilePath);
            }
        }
        catch
        {
            // 迁移失败不影响启动
        }
    }
}