using System.IO;
using System.Text.Json;
using VSCodeTeleporter.Core;

namespace VSCodeTeleporter;

internal static class SettingsLoader
{
    private const string FileName = "appsettings.json";

    public static AppSettings Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, FileName);
        if (!File.Exists(path))
            return new AppSettings();

        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<AppSettings>(json,
                       new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                   ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }
}
