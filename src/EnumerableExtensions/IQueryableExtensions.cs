namespace EnumerableExtensions;

public static class IQueryableExtensions
{
    public static IQueryable<object> SelectPartially<T>(this IQueryable<T> source, ICollection<string> propertyNames)
    {
        return source.Select(ProjectionBuilder<T>.Build(propertyNames));
    }
}
