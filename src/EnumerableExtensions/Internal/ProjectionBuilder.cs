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

    private static Expression<Func<T, object>> BuildInternal(ICollection<string> memberNames, ProjectionOptions options)
    {
        Dictionary<string, MemberInfo> sourceMembers = typeof(T).GetMembers()
            .Where(mi => mi.MemberType is MemberTypes.Property or MemberTypes.Field)
            .Where(pi => memberNames.Contains(pi.Name))
            .ToDictionary(pi => pi.Name, pi => pi);

        Type dynamicType = DynamicTypeBuilder.GetOrBuildDynamicType(new()
        {
            Members = sourceMembers.Select(kv => ToDynamicTypeMember(kv.Key, kv.Value, options)).ToList(),
        });

        ParameterExpression sourceItem = Expression.Parameter(typeof(T), "x");

        IEnumerable<MemberBinding> bindings = dynamicType.GetMembers()
            .Where(mi => mi.MemberType is MemberTypes.Property or MemberTypes.Field)
            .Select(p => Expression.Bind(p, Expression.MakeMemberAccess(sourceItem, sourceMembers[p.Name])));

        return Expression.Lambda<Func<T, object>>(Expression.MemberInit(Expression.New(dynamicType), bindings), sourceItem);
    }

    private static DynamicTypeMember ToDynamicTypeMember(string name, MemberInfo memberInfo, ProjectionOptions options)
        => memberInfo switch
        {
            FieldInfo fieldInfo => new DynamicTypeMember()
            {
                Name = name,
                Type = fieldInfo.FieldType,
                MemberType = options.MemberTypeAsSource
                    ? DynamicTypeMemberType.Field
                    : options.DestinationMemberType,
            },

            PropertyInfo propertyInfo => new DynamicTypeMember()
            {
                Name = name,
                Type = propertyInfo.PropertyType,
                MemberType = options.MemberTypeAsSource
                    ? DynamicTypeMemberType.Property
                    : options.DestinationMemberType,
            },

            _ => throw new NotImplementedException("Only fields and properties can be projected."),
        };

    private static string GetProjectionKey(IEnumerable<string> properties)
        => string.Join(',', properties.OrderBy(p => p));
}
