using EnumerableExtensions.Common;

namespace EnumerableExtensions.Internal;

/// <summary>
/// Represents the specification used to define a dynamically generated type.
/// </summary>
public class TypeSpec : ValueObject
{
    /// <summary>
    /// Gets the name of the type specification.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Gets the base type of the dynamically generated type, if any.
    /// </summary>
    public Type? BaseType { get; init; }

    /// <summary>
    /// Gets the collection of member specifications to include in the type.
    /// </summary>
    required public ICollection<MemberSpec> Members { get; init; }

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return this.Name;
        yield return this.BaseType;

        foreach (MemberSpec member in this.Members)
        {
            yield return member;
        }
    }
}
