using System.Reflection;
using VSCodeTeleporter.Views;

namespace VSCodeTeleporter.Tests;

public class SearchWindowTests
{
    private static bool InvokeIsGitUrl(string? inputString)
    {
        var mi = typeof(SearchWindow).GetMethod("IsGitUrl", BindingFlags.Static | BindingFlags.NonPublic)!;
        return (bool)mi.Invoke(null, [inputString])!;
    }

    [Theory]
    [InlineData("https://github.com/user/repo.git", true)]
    [InlineData("http://example.com/repo.git", true)]
    [InlineData("https://github.com/user/repo", false)]
    [InlineData("not a url.git", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("HTTPS://github.com/user/repo.GIT", true)]
    [InlineData("ssh://git@github.com:user/repo.git", false)]
    public void IsGitUrl_VariousCases(string? input, bool expected)
    {
        var actual = InvokeIsGitUrl(input);
        Assert.Equal(expected, actual);
    }
}
