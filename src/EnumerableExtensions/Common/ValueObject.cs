namespace EnumerableExtensions.Common;

/// <summary> Represents a base class for value objects, enabling value-based equality and hash code generation. </summary>
public abstract class ValueObject
{
    public static bool operator ==(ValueObject a, ValueObject b)
    {
        if (a is null && b is null)
        {
            return true;
        }

        if (a is null || b is null)
        {
            return false;
        }

        return a.Equals(b);
    }

    public static bool operator !=(ValueObject a, ValueObject b)
    {
        return !(a == b);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (this.GetType() != obj.GetType())
        {
            return false;
        }

        var valueObject = (ValueObject)obj;

        return this.GetEqualityComponents().SequenceEqual(valueObject.GetEqualityComponents());
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return this.GetEqualityComponents()
            .Aggregate(1, (current, obj) =>
            {
                unchecked
                {
                    return (current * 23) + (obj?.GetHashCode() ?? 0);
                }
            });
    }

    /// <summary> Provides the components that determine equality for the <see cref="ValueObject"/>. </summary>
    /// <returns> An <see cref="IEnumerable{T}"/> of objects that are used to determine equality. </returns>
    protected abstract IEnumerable<object?> GetEqualityComponents();
}
