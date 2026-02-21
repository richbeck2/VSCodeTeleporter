using Microsoft.Win32;

namespace VSCodeTeleporter;

internal static class StartupManager
{
    private const string RegKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "VSCodeTeleporter";

    public static void SetStartWithWindows(bool enable)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RegKey, writable: true);
        if (key is null) return;

        if (enable)
        {
            var exePath = Environment.ProcessPath ?? 
                          System.Reflection.Assembly.GetExecutingAssembly().Location;
            key.SetValue(AppName, $"\"{exePath}\"");
        }
        else
        {
            key.DeleteValue(AppName, throwOnMissingValue: false);
        }
    }
}
