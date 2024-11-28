using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace EnumerableExtensions;

public static class ProjectionBuilder<T>
{
    private static readonly ConcurrentDictionary<string, Expression<Func<T, object>>> Cache = new();

    public static Expression<Func<T, object>> Build(ICollection<string> properties)
    {
        string key = GetProjectionKey(properties);
        return Cache.GetOrAdd(key, (_) => BuildInternal(properties));
    }

    private static Expression<Func<T, object>> BuildInternal(ICollection<string> properties)
    {
        Dictionary<string, PropertyInfo> sourceProperties = typeof(T).GetProperties()
            .Where(pi => properties.Contains(pi.Name))
            .ToDictionary(pi => pi.Name, pi => pi);

        Type dynamicType = DynamicTypeBuilder.GetDynamicType(sourceProperties.ToDictionary(kv => kv.Key, kv => kv.Value.PropertyType));

        ParameterExpression sourceItem = Expression.Parameter(typeof(T), "x");
        IEnumerable<MemberBinding> bindings = dynamicType.GetFields()
            .Select(p => Expression.Bind(p, Expression.Property(sourceItem, sourceProperties[p.Name])));

        return Expression.Lambda<Func<T, object>>(Expression.MemberInit(Expression.New(dynamicType), bindings), sourceItem);
    }

    private static string GetProjectionKey(IEnumerable<string> properties)
        => string.Join(',', properties.OrderBy(p => p));
}
