using EnumerableExtensions.Common;
using System.Reflection;

namespace EnumerableExtensions.Internal;

/// <summary>
/// Represents the specification for a member (field or property) of a dynamically generated type.
/// </summary>
public class DynamicTypeMember : ValueObject
{
    /// <summary>
    /// Gets the name of the member.
    /// </summary>
    required public string Name { get; init; }

    /// <summary>
    /// Gets the member type, indicating whether it is a field or property.
    /// </summary>
    public MemberTypes MemberType { get; init; }

    /// <summary>
    /// Gets the type of the member.
    /// </summary>
    public Type? Type { get; init; }

    /// <summary>
    /// Gets the dynamic type specification.
    /// </summary>
    public DynamicType? TypeSpec { get; init; }

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return this.Name;
        yield return this.MemberType;
        yield return this.Type;
        yield return this.TypeSpec;
    }
}
