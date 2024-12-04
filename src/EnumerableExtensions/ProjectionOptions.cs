﻿using EnumerableExtensions.Internal;

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
}
