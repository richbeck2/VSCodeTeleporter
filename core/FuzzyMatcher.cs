namespace VSCodeTeleporter.Core;

/// <summary>
/// Lightweight in-process fuzzy matcher.
/// Scoring tiers (higher = better match):
///   3 – exact match (case-insensitive)
///   2 – prefix match
///   1 – subsequence match (all query chars appear in order)
///   0 – no match
/// Within each tier, shorter names rank higher (less noise).
/// </summary>
public class FuzzyMatcher
{
    public IReadOnlyList<string> Filter(IEnumerable<string> candidates, string query)
    {
        if (string.IsNullOrEmpty(query))
            return candidates.ToList();

        var q = query.ToLowerInvariant();

        return candidates
            .Select(c => (Name: c, Score: Score(c.ToLowerInvariant(), q), Len: c.Length))
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Len)
            .Select(x => x.Name)
            .ToList();
    }

    private static int Score(string candidate, string query)
    {
        if (candidate == query) return 3;
        if (candidate.StartsWith(query)) return 2;
        if (IsSubsequence(candidate, query)) return 1;
        return 0;
    }

    private static bool IsSubsequence(string candidate, string query)
    {
        int ci = 0, qi = 0;
        while (ci < candidate.Length && qi < query.Length)
        {
            if (candidate[ci] == query[qi]) qi++;
            ci++;
        }
        return qi == query.Length;
    }
}
