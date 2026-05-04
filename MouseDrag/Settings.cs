using System.Text.Json;
using System.Text.Json.Serialization;

namespace MouseDrag;

public class AppSettings
{
    public bool Enabled { get; set; } = true;
    public int ButtonCount { get; set; } = 3;
    public int HotkeyModifiers { get; set; } = 6; // Ctrl+Shift
    public int HotkeyKey { get; set; } = 0x44; // 'D'
    public string Language { get; set; } = "zh-CN";
    public List<MouseButton> ButtonCombo { get; set; } = new()
    {
        MouseButton.Left,
        MouseButton.Right,
        MouseButton.XButton1
    };

    private static readonly string SettingsDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "MouseDrag");
    private static readonly string SettingsPath = Path.Combine(SettingsDir, "settings.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                return JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
            }
        }
        catch { }
        return new AppSettings();
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(SettingsDir);
            var json = JsonSerializer.Serialize(this, JsonOptions);
            File.WriteAllText(SettingsPath, json);
        }
        catch { }
    }
}
