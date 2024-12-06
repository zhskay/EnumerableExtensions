namespace EnumerableExtensions.Internal;

/// <summary>
/// Represents the specification for a member (field or property) of a dynamically generated type.
/// </summary>
public class DynamicTypeMember
{
    /// <summary>
    /// Gets the name of the member.
    /// </summary>
    required public string Name { get; init; }

    /// <summary>
    /// Gets the type of the member.
    /// </summary>
    required public Type Type { get; init; }

    /// <summary>
    /// Gets the member type, indicating whether it is a field or property.
    /// </summary>
    public DynamicTypeMemberType MemberType { get; init; }
}
