namespace EnumerableExtensions;

/// <summary>
/// Specifies the type of a member in a dynamically generated type.
/// </summary>
public enum ProjectMemberType
{
    /// <summary>
    /// Represents a member that is a field.
    /// </summary>
    Field,

    /// <summary>
    /// Represents a member that is a property.
    /// </summary>
    Property,

    /// <summary>
    /// Represents a value indicating whether the member type (field or property) in the dynamically generated class
    /// should match the type of the corresponding member in the source type.
    /// </summary>
    AsSource,
}
