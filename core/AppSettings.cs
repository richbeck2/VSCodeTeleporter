namespace VSCodeTeleporter.Core;

public class AppSettings
{
    public string SearchRoot { get; set; } = @"C:\src\";
    public string HotkeyModifiers { get; set; } = "Ctrl+Alt";
    public string HotkeyKey { get; set; } = "S";
    public bool StartWithWindows { get; set; } = false;
}
