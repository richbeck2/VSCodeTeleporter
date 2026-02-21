namespace VSCodeTeleporter.Core;

public static class SelectionHelper
{
    /// <summary>
    /// Computes the next selected index when moving up/down in a circular list.
    /// Mirrors the logic used by `SearchWindow.MoveSelection`.
    /// </summary>
    public static int NextIndex(int currentIndex, int delta, int count)
    {
        if (count <= 0) return -1;
        return (currentIndex + delta + count) % count;
    }
}
