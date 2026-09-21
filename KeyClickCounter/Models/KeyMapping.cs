namespace KeyClickCounter.Models;

/// <summary>Win32 虚拟键码 → 界面按键 Id 映射表。</summary>
public static class KeyMapping
{
    private static readonly Dictionary<uint, string> Map = new();

    static KeyMapping()
    {
        Map[0x08] = "Backspace";
        Map[0x09] = "Tab";
        Map[0x0D] = "Enter";        // 非扩展回车；小键盘回车由钩子按扩展位单独识别
        Map[0x13] = "Pause";
        Map[0x14] = "CapsLock";
        Map[0x1B] = "Esc";
        Map[0x20] = "Space";
        Map[0x21] = "PgUp";
        Map[0x22] = "PgDn";
        Map[0x23] = "End";
        Map[0x24] = "Home";
        Map[0x25] = "Left";
        Map[0x26] = "Up";
        Map[0x27] = "Right";
        Map[0x28] = "Down";
        Map[0x2C] = "PrtSc";
        Map[0x2D] = "Ins";
        Map[0x2E] = "Del";
        for (uint i = 0; i < 10; i++) Map[0x30 + i] = ((char)('0' + i)).ToString();
        for (uint i = 0; i < 26; i++) Map[0x41 + i] = ((char)('A' + i)).ToString();
        Map[0x5B] = "Win";          // 左右 Win 合并计入同一格
        Map[0x5C] = "Win";
        Map[0x5D] = "Menu";
        for (uint i = 0; i < 10; i++) Map[0x60 + i] = "Num" + i;
        Map[0x6A] = "NumMul";
        Map[0x6B] = "NumAdd";
        Map[0x6D] = "NumSub";
        Map[0x6E] = "NumDec";
        Map[0x6F] = "NumDiv";
        for (uint i = 0; i < 12; i++) Map[0x70 + i] = "F" + (i + 1);
        Map[0x90] = "NumLock";
        Map[0x91] = "ScrLk";
        Map[0xA0] = "LShift";
        Map[0xA1] = "RShift";
        Map[0xA2] = "LCtrl";
        Map[0xA3] = "RCtrl";
        Map[0xA4] = "LAlt";
        Map[0xA5] = "RAlt";
        Map[0xBA] = ";";
        Map[0xBB] = "=";
        Map[0xBC] = ",";
        Map[0xBD] = "-";
        Map[0xBE] = ".";
        Map[0xBF] = "/";
        Map[0xC0] = "`";
        Map[0xDB] = "[";
        Map[0xDC] = "\\";
        Map[0xDD] = "]";
        Map[0xDE] = "'";
        Map[0xE2] = "\\";           // ISO 布局附加反斜杠键
    }

    public static string? GetId(uint vk) => Map.TryGetValue(vk, out var id) ? id : null;
}