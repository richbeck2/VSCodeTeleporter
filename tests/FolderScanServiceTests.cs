using VSCodeTeleporter.Core;

namespace VSCodeTeleporter.Tests;

public class FolderScanServiceTests : IDisposable
{
    private readonly string _tempRoot;
    private readonly FolderScanService _service = new();

    public FolderScanServiceTests()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), $"OvscTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempRoot);

        // 4 sub-folders
        Directory.CreateDirectory(Path.Combine(_tempRoot, "Alpha"));
        Directory.CreateDirectory(Path.Combine(_tempRoot, "Beta"));
        Directory.CreateDirectory(Path.Combine(_tempRoot, "Gamma"));
        Directory.CreateDirectory(Path.Combine(_tempRoot, "Delta"));

        // 1 file that should be ignored
        File.WriteAllText(Path.Combine(_tempRoot, "README.md"), "# test");
    }

    [Fact]
    public void GetFolders_ReturnsExactlyFourFolders()
    {
        var result = _service.GetFolders(_tempRoot);
        Assert.Equal(4, result.Count);
    }

    [Fact]
    public void GetFolders_DoesNotIncludeFiles()
    {
        var result = _service.GetFolders(_tempRoot);
        Assert.DoesNotContain("README.md", result);
    }

    [Fact]
    public void GetFolders_ContainsExpectedNames()
    {
        var result = _service.GetFolders(_tempRoot);
        Assert.Contains("Alpha", result);
        Assert.Contains("Beta", result);
        Assert.Contains("Gamma", result);
        Assert.Contains("Delta", result);
    }

    [Fact]
    public void GetFolders_ReturnsEmpty_WhenRootDoesNotExist()
    {
        var result = _service.GetFolders(@"C:\__this_path_should_not_exist__\");
        Assert.Empty(result);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
            Directory.Delete(_tempRoot, recursive: true);
    }
}
