using EnumerableExtensions.Internal;

namespace EnumerableExtensions;

/// <summary>
/// Extension methods for <see cref="IQueryable{T}" />.
/// </summary>
public static class IQueryableExtensions
{
    /// <summary>
    /// Projects each element of sequence into a new form that contains only specific fields.
    /// </summary>
    /// <typeparam name="T"> The type of the data in the data source. </typeparam>
    /// <param name="source"> The source enumerable. </param>
    /// <param name="propertyNames"> List of property names of the source type that should be projected. </param>
    /// <returns> An <see cref="IQueryable{object}"/> whose elements contain only specified fields. </returns>
    public static IQueryable<object> SelectPartially<T>(this IQueryable<T> source, ICollection<string> propertyNames)
    {
        return source.Select(ProjectionBuilder<T>.Build(propertyNames));
    }
}
