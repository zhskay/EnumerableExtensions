namespace EnumerableExtensions.Internal;

/// <summary>
/// Represents the specification used to define a dynamically generated type.
/// </summary>
public class DynamicType
{
    /// <summary>
    /// Gets the name of the dynamic type.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Gets the base type of the dynamic type, if any.
    /// </summary>
    public Type? BaseType { get; init; }

    /// <summary>
    /// Gets the collection of members (fields or properties) to include in the dynamic type.
    /// </summary>
    required public ICollection<DynamicTypeMember> Members { get; init; }

    /// <summary>
    /// Validates specification.
    /// </summary>
    /// <param name="paramName"> The parameter name to include in the exception message, if validation fails. </param>
    public void Validate(string paramName = "")
    {
        ArgumentOutOfRangeException.ThrowIfZero(this.Members.Count, string.Join('.', paramName, nameof(this.Members)));
    }
}
