namespace KeyClickCounter.Models;

/// <summary>标准 104 键布局 + 5 个鼠标键的几何定义（单位 u = 50px）。</summary>
public static class KeyboardLayout
{
    public const double Unit = 50;
    public const double Gap = 3;

    public static double CanvasWidth => 22.75 * Unit;

    public static double CanvasHeight => 6 * Unit;

    private static KeyItem K(string id, string name, uint vk, double ux, double uy,
                             double uw, double uh = 1, string? sub = null, bool isFn = false)
        => new(id, name, vk, ux * Unit + Gap, uy * Unit + Gap,
               uw * Unit - 2 * Gap, uh * Unit - 2 * Gap, sub, false, isFn);

    public static List<KeyItem> BuildKeyboardKeys()
    {
        var list = new List<KeyItem>(110);

        // ── 功能键区 ──
        list.Add(K("Esc", "Esc", 0x1B, 0, 0, 1));
        for (int i = 0; i < 4; i++) list.Add(K("F" + (i + 1), "F" + (i + 1), (uint)(0x70 + i), 2 + i, 0, 1));
        for (int i = 0; i < 4; i++) list.Add(K("F" + (i + 5), "F" + (i + 5), (uint)(0x74 + i), 6.5 + i, 0, 1));
        for (int i = 0; i < 4; i++) list.Add(K("F" + (i + 9), "F" + (i + 9), (uint)(0x78 + i), 11 + i, 0, 1));
        list.Add(K("PrtSc", "PrtSc", 0x2C, 15.5, 0, 1));
        list.Add(K("ScrLk", "ScrLk", 0x91, 16.5, 0, 1));
        list.Add(K("Pause", "Pause", 0x13, 17.5, 0, 1));

        // ── 主键盘区：数字行 ──
        const string digits = "1234567890";
        const string digSub = "!@#$%^&*()";
        list.Add(K("`", "`", 0xC0, 0, 1, 1, 1, "~"));
        for (int i = 0; i < 10; i++)
            list.Add(K(digits[i].ToString(), digits[i].ToString(), digits[i], 1 + i, 1, 1, 1, digSub[i].ToString()));
        list.Add(K("-", "-", 0xBD, 11, 1, 1, 1, "_"));
        list.Add(K("=", "=", 0xBB, 12, 1, 1, 1, "+"));
        list.Add(K("Backspace", "Backspace", 0x08, 13, 1, 2));

        // Tab 行
        list.Add(K("Tab", "Tab", 0x09, 0, 2, 1.5));
        AddLetterRow(list, "QWERTYUIOP", 1.5, 2);
        list.Add(K("[", "[", 0xDB, 11.5, 2, 1, 1, "{"));
        list.Add(K("]", "]", 0xDD, 12.5, 2, 1, 1, "}"));
        list.Add(K("\\", "\\", 0xDC, 13.5, 2, 1.5, 1, "|"));

        // Caps 行
        list.Add(K("CapsLock", "Caps Lock", 0x14, 0, 3, 1.75));
        AddLetterRow(list, "ASDFGHJKL", 1.75, 3);
        list.Add(K(";", ";", 0xBA, 10.75, 3, 1, 1, ":"));
        list.Add(K("'", "'", 0xDE, 11.75, 3, 1, 1, "\""));
        list.Add(K("Enter", "Enter", 0x0D, 12.75, 3, 2.25));

        // Shift 行
        list.Add(K("LShift", "Shift", 0xA0, 0, 4, 2.25));
        AddLetterRow(list, "ZXCVBNM", 2.25, 4);
        list.Add(K(",", ",", 0xBC, 9.25, 4, 1, 1, "<"));
        list.Add(K(".", ".", 0xBE, 10.25, 4, 1, 1, ">"));
        list.Add(K("/", "/", 0xBF, 11.25, 4, 1, 1, "?"));
        list.Add(K("RShift", "Shift", 0xA1, 12.25, 4, 2.75));

        // 底行
        list.Add(K("LCtrl", "Ctrl", 0xA2, 0, 5, 1.25));
        list.Add(K("Win", "Win", 0x5B, 1.25, 5, 1.25));
        list.Add(K("LAlt", "Alt", 0xA4, 2.5, 5, 1.25));
        list.Add(K("Space", "Space", 0x20, 3.75, 5, 6.25));
        list.Add(K("RAlt", "Alt", 0xA5, 10, 5, 1.25));
        list.Add(K("Fn", "Fn", 0, 11.25, 5, 1.25, 1, null, true));
        list.Add(K("Menu", "Menu", 0x5D, 12.5, 5, 1.25));
        list.Add(K("RCtrl", "Ctrl", 0xA3, 13.75, 5, 1.25));

        // ── 编辑键区 ──
        list.Add(K("Ins", "Ins", 0x2D, 15.5, 2, 1));
        list.Add(K("Home", "Home", 0x24, 16.5, 2, 1));
        list.Add(K("PgUp", "PgUp", 0x21, 17.5, 2, 1));
        list.Add(K("Del", "Del", 0x2E, 15.5, 3, 1));
        list.Add(K("End", "End", 0x23, 16.5, 3, 1));
        list.Add(K("PgDn", "PgDn", 0x22, 17.5, 3, 1));
        list.Add(K("Up", "↑", 0x26, 16.5, 4, 1));
        list.Add(K("Left", "←", 0x25, 15.5, 5, 1));
        list.Add(K("Down", "↓", 0x28, 16.5, 5, 1));
        list.Add(K("Right", "→", 0x27, 17.5, 5, 1));

        // ── 数字小键盘区 ──
        list.Add(K("NumLock", "Num", 0x90, 18.75, 1, 1));
        list.Add(K("NumDiv", "/", 0x6F, 19.75, 1, 1));
        list.Add(K("NumMul", "*", 0x6A, 20.75, 1, 1));
        list.Add(K("NumSub", "-", 0x6D, 21.75, 1, 1));
        list.Add(K("Num7", "7", 0x67, 18.75, 2, 1, 1, "Home"));
        list.Add(K("Num8", "8", 0x68, 19.75, 2, 1, 1, "↑"));
        list.Add(K("Num9", "9", 0x69, 20.75, 2, 1, 1, "PgUp"));
        list.Add(K("NumAdd", "+", 0x6B, 21.75, 2, 1, 2));
        list.Add(K("Num4", "4", 0x64, 18.75, 3, 1, 1, "←"));
        list.Add(K("Num5", "5", 0x65, 19.75, 3, 1));
        list.Add(K("Num6", "6", 0x66, 20.75, 3, 1, 1, "→"));
        list.Add(K("Num1", "1", 0x61, 18.75, 4, 1, 1, "End"));
        list.Add(K("Num2", "2", 0x62, 19.75, 4, 1, 1, "↓"));
        list.Add(K("Num3", "3", 0x63, 20.75, 4, 1, 1, "PgDn"));
        list.Add(K("NumEnter", "Enter", 0x0D, 21.75, 4, 1, 2));
        list.Add(K("Num0", "0", 0x60, 18.75, 5, 2, 1, "Ins"));
        list.Add(K("NumDec", ".", 0x6E, 20.75, 5, 1, 1, "Del"));

        return list;
    }

    private static void AddLetterRow(List<KeyItem> list, string letters, double startX, int row)
    {
        for (int i = 0; i < letters.Length; i++)
        {
            string s = letters[i].ToString();
            list.Add(K(s, s, letters[i], startX + i, row, 1));
        }
    }

    public static List<KeyItem> BuildMouseKeys() => new()
    {
        new KeyItem("MouseLeft",   "Left",  0x01, 0, 0, 150, 56, null, true),
        new KeyItem("MouseRight",  "Right",  0x02, 0, 0, 150, 56, null, true),
        new KeyItem("MouseMiddle", "Middle",  0x04, 0, 0, 150, 56, null, true),
        new KeyItem("MouseX1",     "X1", 0x05, 0, 0, 150, 56, null, true),
        new KeyItem("MouseX2",     "X2", 0x06, 0, 0, 150, 56, null, true),
        new KeyItem("MouseWheel",  "Wheel",  0,    0, 0, 150, 56, null, true),
    };
/// <summary>
    /// 按当前界面语言更新键帽主标识（含鼠标键名）。
    /// 语言词典中用 L.Key.&lt;Id&gt; 提供本地化名称；未提供的键保留默认英文标注。
    /// </summary>
    public static void ApplyLanguage(IEnumerable<KeyItem> items)
    {
        foreach (var k in items)
        {
            string text = Services.LocalizationService.GetText("Key." + k.Id);
            if (text != "L.Key." + k.Id) k.SetDisplayName(text);
        }
    }
}