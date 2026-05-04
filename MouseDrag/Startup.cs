using Microsoft.Win32;

namespace MouseDrag;

public static class Startup
{
    private const string AppName = "MouseDrag";
    private static readonly string ExePath = Environment.ProcessPath!;

    public static bool IsEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", false);
            return key?.GetValue(AppName)?.ToString() == $"\"{ExePath}\"";
        }
        catch { return false; }
    }

    public static void SetEnabled(bool enable)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            if (key == null) return;
            if (enable)
                key.SetValue(AppName, $"\"{ExePath}\"");
            else
                key.DeleteValue(AppName, false);
        }
        catch { }
    }
}
