namespace EnumerableExtensions.Internal;

/// <summary>
/// Specifies the type of a member in a dynamically generated type.
/// </summary>
public enum DynamicTypeMemberType
{
    /// <summary>
    /// Represents a member that is a field.
    /// </summary>
    Field,

    /// <summary>
    /// Represents a member that is a property.
    /// </summary>
    Property,
}
