using VSCodeTeleporter.Core;

namespace VSCodeTeleporter.Tests;

public class FuzzyMatcherTests
{
    private readonly FuzzyMatcher _matcher = new();
    private readonly string[] _names = ["Alpha", "AlphaTwo", "BetaAlpha", "Gamma", "Delta"];

    [Fact]
    public void Filter_EmptyQuery_ReturnsAll()
    {
        var result = _matcher.Filter(_names, "");
        Assert.Equal(_names.Length, result.Count);
    }

    [Fact]
    public void Filter_ExactPrefix_RanksAboveMidString()
    {
        // "Alpha" is a prefix of "Alpha" and "AlphaTwo"; "BetaAlpha" contains it mid-string.
        var results = _matcher.Filter(_names, "Alpha").ToList();
        // First two results must be exact/prefix matches, mid-string must come after
        var betaAlphaIndex = results.IndexOf("BetaAlpha");
        var alphaIndex = results.IndexOf("Alpha");
        Assert.True(alphaIndex < betaAlphaIndex,
            "Prefix match 'Alpha' should rank above mid-string match 'BetaAlpha'");
    }

    [Fact]
    public void Filter_NoMatch_ReturnsEmpty()
    {
        var result = _matcher.Filter(_names, "zzz");
        Assert.Empty(result);
    }

    [Fact]
    public void Filter_SubsequenceMatch_ReturnsCandidates()
    {
        // "gm" is a subsequence of "Gamma"
        var result = _matcher.Filter(["Gamma", "Delta"], "gm");
        Assert.Contains("Gamma", result);
        Assert.DoesNotContain("Delta", result);
    }

    [Fact]
    public void Filter_CaseInsensitive()
    {
        var result = _matcher.Filter(["Alpha", "Beta"], "alpha");
        Assert.Contains("Alpha", result);
    }
}
