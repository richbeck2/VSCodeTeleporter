namespace VSCodeTeleporter.Core;

public class FolderScanService
{
    /// <summary>
    /// Returns the names (not full paths) of the top-level directories
    /// directly inside <paramref name="rootPath"/>.  Files are ignored.
    /// </summary>
    public IReadOnlyList<string> GetFolders(string rootPath)
    {
        if (!Directory.Exists(rootPath))
            return [];

        return Directory
            .EnumerateDirectories(rootPath, "*", SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileName)
            .Where(n => n is not null)
            .Select(n => n!)
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
