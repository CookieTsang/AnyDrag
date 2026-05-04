namespace AnyDrag;

public static class Lang
{
    private static string _current = "zh-CN";

    private static readonly Dictionary<string, Dictionary<string, string>> Strings = new()
    {
        ["zh-CN"] = new()
        {
            // Tray menu
            ["TrayEnabled"] = "启用",
            ["TrayStartup"] = "开机启动",
            ["TraySettings"] = "设置",
            ["TrayExit"] = "退出",
            ["TrayEnabledTip"] = "已启用",
            ["TrayDisabledTip"] = "已禁用",

            // Settings form
            ["SettingsTitle"] = "AnyDrag 设置",
            ["ButtonCount"] = "按键数量：",
            ["CurrentCombo"] = "当前按键组合：",
            ["RecordCombo"] = "录制新组合",
            ["ResetDefault"] = "恢复默认",
            ["ToggleHotkey"] = "切换快捷键：",
            ["RecordHotkey"] = "录制快捷键",
            ["Ok"] = "确定",
            ["Cancel"] = "取消",
            ["NotSet"] = "（未设置）",
            ["PressCombo"] = "请按下组合键...",
            ["PressHotkey"] = "请按下快捷键...",
            ["Language"] = "语言：",

            // Mouse buttons
            ["Left"] = "左键",
            ["Right"] = "右键",
            ["Middle"] = "中键",
            ["XButton1"] = "侧键1",
            ["XButton2"] = "侧键2",
        },
        ["en-US"] = new()
        {
            // Tray menu
            ["TrayEnabled"] = "Enabled",
            ["TrayStartup"] = "Start on boot",
            ["TraySettings"] = "Settings",
            ["TrayExit"] = "Exit",
            ["TrayEnabledTip"] = "Enabled",
            ["TrayDisabledTip"] = "Disabled",

            // Settings form
            ["SettingsTitle"] = "AnyDrag Settings",
            ["ButtonCount"] = "Button count:",
            ["CurrentCombo"] = "Current combo:",
            ["RecordCombo"] = "Record Combo",
            ["ResetDefault"] = "Reset Default",
            ["ToggleHotkey"] = "Toggle hotkey:",
            ["RecordHotkey"] = "Record Hotkey",
            ["Ok"] = "OK",
            ["Cancel"] = "Cancel",
            ["NotSet"] = "(Not set)",
            ["PressCombo"] = "Press buttons...",
            ["PressHotkey"] = "Press hotkey...",
            ["Language"] = "Language:",

            // Mouse buttons
            ["Left"] = "Left",
            ["Right"] = "Right",
            ["Middle"] = "Middle",
            ["XButton1"] = "Side1",
            ["XButton2"] = "Side2",
        }
    };

    public static void SetLanguage(string culture)
    {
        _current = Strings.ContainsKey(culture) ? culture : "zh-CN";
    }

    public static string Get(string key)
    {
        if (Strings.TryGetValue(_current, out var dict) && dict.TryGetValue(key, out var val))
            return val;
        if (Strings["zh-CN"].TryGetValue(key, out var fallback))
            return fallback;
        return key;
    }

    public static string MouseButtonName(MouseButton button)
    {
        return button switch
        {
            MouseButton.Left => Get("Left"),
            MouseButton.Right => Get("Right"),
            MouseButton.Middle => Get("Middle"),
            MouseButton.XButton1 => Get("XButton1"),
            MouseButton.XButton2 => Get("XButton2"),
            _ => button.ToString()
        };
    }

    public static string FormatCombo(IEnumerable<MouseButton> buttons)
    {
        return string.Join(" + ", buttons.Select(MouseButtonName));
    }

    public static string[] AvailableLanguages => Strings.Keys.ToArray();
}
