using VSCodeTeleporter.Core;

namespace VSCodeTeleporter.Tests;

public class SelectionTests
{
    [Theory]
    [InlineData(0, 1, 5, 1)]
    [InlineData(0, -1, 5, 4)]
    [InlineData(2, 1, 5, 3)]
    [InlineData(4, 1, 5, 0)]
    [InlineData(-1, 1, 5, 0)]
    [InlineData(-1, -1, 5, 3)]
    public void NextIndex_WrapsAndAdvances(int current, int delta, int count, int expected)
    {
        var next = SelectionHelper.NextIndex(current, delta, count);
        Assert.Equal(expected, next);
    }

    [Fact]
    public void NextIndex_ReturnsMinusOne_ForEmpty()
    {
        Assert.Equal(-1, SelectionHelper.NextIndex(0, 1, 0));
    }
}
