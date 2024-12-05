using EnumerableExtensions.Internal;

namespace EnumerableExtensions;

/// <summary>
/// Represents options used to configure the behavior of the <c>SelectPartially</c> method.
/// </summary>
public class ProjectionOptions
{
    /// <summary>
    /// Gets the type of members (fields or properties) to be created in the dynamically generated class during projection.
    /// </summary>
    /// <remarks>
    /// This specifies whether the dynamically generated class will have fields or properties for its members.
    /// The default value is <see cref="DynamicTypeMemberType.Field" />.
    /// </remarks>
    public DynamicTypeMemberType DestinationMemberType { get; init; } = DynamicTypeMemberType.Field;

    /// <summary>
    /// Gets a value indicating whether the member type (field or property) in the dynamically generated class
    /// should match the type of the corresponding member in the source type.
    /// </summary>
    /// <remarks>
    /// If set to <c>true</c>, the generated class will use the same member type as the source type (e.g., fields remain fields, properties remain properties). 
    /// If <c>false</c>, the type of members is determined by <see cref="DestinationMemberType" />.
    /// The default value is <c>false</c>.
    /// </remarks>
    public bool MemberTypeAsSource { get; init; } = false;
}
