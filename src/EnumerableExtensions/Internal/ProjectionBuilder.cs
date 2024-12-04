using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace EnumerableExtensions.Internal;

/// <summary>
/// Generates and cache <see cref="Expression{Func{T, object}}"/> for projections.
/// </summary>
/// <typeparam name="T"> The type of the data in the data source. </typeparam>
public static class ProjectionBuilder<T>
{
    private static readonly ConcurrentDictionary<string, Expression<Func<T, object>>> Cache = new();

    /// <summary>
    /// Returns <see cref="Expression{Func{T, object}}"/> that projects element of the source type to type that contains only specified fields.
    /// </summary>
    /// <param name="properties"> List of property names of the source type that should be projected. </param>
    /// <param name="options"> Projection options. </param>
    /// <returns> An <see cref="Expression{Func{T, object}}"/> that projects element of the source type to type that contains only specified fields. </returns>
    public static Expression<Func<T, object>> Build(ICollection<string> properties, ProjectionOptions options)
    {
        string key = GetProjectionKey(properties);
        return Cache.GetOrAdd(key, (_) => BuildInternal(properties, options));
    }

    private static Expression<Func<T, object>> BuildInternal(ICollection<string> properties, ProjectionOptions options)
    {
        Dictionary<string, PropertyInfo> sourceProperties = typeof(T).GetProperties()
            .Where(pi => properties.Contains(pi.Name))
            .ToDictionary(pi => pi.Name, pi => pi);

        Type dynamicType = DynamicTypeBuilder.GetOrBuildDynamicType(new()
        {
            Members = sourceProperties
                .Select(kv => new DynamicTypeMember()
                {
                    Name = kv.Key,
                    Type = kv.Value.PropertyType,
                    MemberType = options.DestinationMemberType,
                })
                .ToList(),
        });

        ParameterExpression sourceItem = Expression.Parameter(typeof(T), "x");
        IEnumerable<MemberBinding> bindings = dynamicType.GetFields()
            .Select(p => Expression.Bind(p, Expression.Property(sourceItem, sourceProperties[p.Name])));

        return Expression.Lambda<Func<T, object>>(Expression.MemberInit(Expression.New(dynamicType), bindings), sourceItem);
    }

    private static string GetProjectionKey(IEnumerable<string> properties)
        => string.Join(',', properties.OrderBy(p => p));
}
