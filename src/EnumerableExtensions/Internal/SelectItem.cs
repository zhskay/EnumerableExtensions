namespace EnumerableExtensions.Internal;

/// <summary>
/// Represents an item in a hierarchical structure parsed from a select expression.
/// </summary>
public class SelectItem
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SelectItem"/> class with a specified name.
    /// </summary>
    /// <param name="name">The name of the select item.</param>
    public SelectItem(string name)
    {
        this.Name = name;
        this.Items = new SortedSet<SelectItem>(new SelectItemComparer(StringComparer.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SelectItem"/> class with a specified name and nested items.
    /// </summary>
    /// <param name="name">The name of the select item.</param>
    /// <param name="items">The nested items contained in this select item.</param>
    public SelectItem(string name, SortedSet<SelectItem> items)
    {
        this.Name = name;
        this.Items = items;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SelectItem"/> class with a specified name and a custom comparer for the nested items.
    /// </summary>
    /// <param name="name">The name of the select item.</param>
    /// <param name="comparer">The comparer used to sort the nested items.</param>
    public SelectItem(string name, IComparer<string> comparer)
    {
        this.Name = name;
        this.Items = new SortedSet<SelectItem>(new SelectItemComparer(comparer));
    }

    /// <summary>
    /// Gets or sets the name of the select item.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the collection of nested <see cref="SelectItem"/> objects within this item.
    /// </summary>
    public SortedSet<SelectItem> Items { get; set; }
}
