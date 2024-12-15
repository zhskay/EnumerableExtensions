namespace EnumerableExtensions.Parsing;

/// <summary>
/// Provides a comparer for <see cref="SelectItem"/> objects based on their names.
/// </summary>
public class SelectItemComparer : IComparer<SelectItem>
{
    private readonly IComparer<string> comparer;

    /// <summary>
    /// Initializes a new instance of the <see cref="SelectItemComparer"/> class with a specified string comparer.
    /// </summary>
    /// <param name="comparer">The comparer used to compare the names of <see cref="SelectItem"/> objects.</param>
    public SelectItemComparer(IComparer<string> comparer)
    {
        this.comparer = comparer;
    }

    /// <inheritdoc/>
    public int Compare(SelectItem? x, SelectItem? y)
        => this.comparer.Compare(x?.Name, y?.Name);
}
