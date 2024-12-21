using EnumerableExtensions.Exceptions;
using EnumerableExtensions.Parsing;
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
    /// <param name="select"> The select expression defining the fields to include in the projection. </param>
    /// <param name="options"> Projection options. </param>
    /// <returns> An <see cref="Expression{Func{T, object}}"/> that projects element of the source type to type that contains only specified fields. </returns>
    public static Expression<Func<T, object>> Build(string select, ProjectionOptions options)
    {
        SortedSet<SelectItem> items = SelectRecursiveParser.ParseSelect(select);

        string key = GetProjectionKey(items);

        return Cache.GetOrAdd(key, (_) => BuildInternal(items, options));
    }

    private static Expression<Func<T, object>> BuildInternal(SortedSet<SelectItem> selectItems, ProjectionOptions options)
    {
        Projection[] projections = CreateProjections(typeof(T), selectItems, options);

        TypeSpec spec = new() { Members = projections.Select(CreateMemberSpec).ToArray() };
        Type dynamicType = DynamicTypeBuilder.GetOrCreateDynamicType(spec);

        ParameterExpression sourceElement = Expression.Parameter(typeof(T), "x");

        // x => new Destination
        // {
        //     ValueType = x.ValueType,
        //     Object = x.Class,
        //     PartialObject = new DestPartialObject
        //     {
        //         ValueType = x.PartialObject.ValueType,
        //         ...
        //     },
        //     Array = x.Array.Select(item => new DestItem { ValueType = item.ValueType, ... })
        // }
        return Expression.Lambda<Func<T, object>>(CreateObjectProjection(sourceElement, dynamicType, projections), sourceElement);
    }

    private static MemberInitExpression CreateObjectProjection(Expression source, Type destType, ICollection<Projection> projections)
    {
        MemberInfo[] destMembers = destType.GetMembers()
            .Where(mi => mi.MemberType is MemberTypes.Property or MemberTypes.Field)
            .ToArray();

        IEnumerable<MemberBinding> bindings = projections
            .Select(projection =>
            {
                MemberInfo destMemberInfo = destMembers.FirstOrDefault(mi => mi.Name == projection.SourceMember.Name)
                    ?? throw new Exception($"Destination type does not have member {projection.SourceMember.Name} for project to.");

                MemberExpression sourceMember = Expression.MakeMemberAccess(source, projection.SourceMember);

                if (projection.InnerProjections is null or { Count: 0 })
                {
                    return Expression.Bind(destMemberInfo, sourceMember);
                }
                else
                {
                    Expression expression = GetUnderlyingType(destMemberInfo) switch
                    {
                        Type valueType when projection.InnerProjections is null or { Count: 0 }
                            => sourceMember,

                        Type enumerableType when GetEnumerableElementType(enumerableType) is Type elementType
                            => Expression.Condition(
                                Expression.Equal(sourceMember, Expression.Constant(null, sourceMember.Type)),
                                Expression.Constant(null, enumerableType),
                                CreateEnumerableProjection(sourceMember, elementType, projection.InnerProjections)),

                        Type classType when classType.IsClass
                            => Expression.Condition(
                                Expression.Equal(sourceMember, Expression.Constant(null, sourceMember.Type)),
                                Expression.Constant(null, classType),
                                CreateObjectProjection(sourceMember, classType, projection.InnerProjections)),

                        _
                            => sourceMember,
                    };

                    return Expression.Bind(destMemberInfo, expression);
                }
            });

        return Expression.MemberInit(Expression.New(destType), bindings);
    }

    private static MethodCallExpression CreateEnumerableProjection(Expression source, Type destElementType, ICollection<Projection> projections)
    {
        if (GetEnumerableElementType(source.Type) is not Type sourceElementType)
        {
            throw new InvalidOperationException();
        }

        ParameterExpression sourceElement = Expression.Parameter(sourceElementType, "item");

        MethodCallExpression expression = Expression.Call(
            null,
            ReflectionHelper.SelectMethodInfo.MakeGenericMethod(sourceElementType, destElementType),
            source,
            Expression.Lambda(CreateObjectProjection(sourceElement, destElementType, projections), sourceElement));

        return expression;
    }

    private static Projection[] CreateProjections(Type type, IEnumerable<SelectItem> selectItems, ProjectionOptions options)
    {
        if (GetEnumerableElementType(type) is Type elementType)
        {
            type = elementType;
        }

        MemberInfo[] members = type.GetMembers(BindingFlags.Public | BindingFlags.Instance)
            .Where(mi => mi.MemberType is MemberTypes.Property or MemberTypes.Field)
            .ToArray();

        return selectItems.OrderBy(x => x.Order).Select(select =>
        {
            MemberInfo member = members.FirstOrDefault(mi => string.Equals(mi.Name, select.Name, StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidSelectItemException(select, type);
            Type memberType = GetUnderlyingType(member);

            ICollection<Projection>? innerProjections = select.Items.Count > 0
                ? CreateProjections(memberType, select.Items, options)
                : null;

            return new Projection
            {
                MemberType = options.MemberType,
                SourceMember = member,
                InnerProjections = innerProjections,
            };
        }).ToArray();
    }

    private static MemberSpec CreateMemberSpec(Projection projection)
    {
        Type sourceMemberType = GetUnderlyingType(projection.SourceMember);
        Type? sourceMemberElementType = GetEnumerableElementType(sourceMemberType);

        return new MemberSpec
        {
            Name = projection.SourceMember.Name,
            MemberType = GetMemberType(projection),
            IsEnumerable = sourceMemberElementType is not null,
            Type = projection.InnerProjections is null
                ? sourceMemberElementType ?? sourceMemberType
                : null,
            TypeSpec = projection.InnerProjections is not null
                ? new TypeSpec { Members = projection.InnerProjections.Select(CreateMemberSpec).ToArray() }
                : null,
        };
    }

    private static Type GetUnderlyingType(MemberInfo member)
        => member switch
        {
            FieldInfo fieldInfo => fieldInfo.FieldType,
            PropertyInfo propertyInfo => propertyInfo.PropertyType,
            _ => throw new NotImplementedException("Only fields and properties can be projected."),
        };

    private static MemberTypes GetMemberType(Projection projection)
        => projection.MemberType switch
        {
            ProjectionType.Field => MemberTypes.Field,
            ProjectionType.Property => MemberTypes.Property,
            _ => projection.SourceMember switch
            {
                FieldInfo => MemberTypes.Field,
                PropertyInfo => MemberTypes.Property,
                _ => throw new NotImplementedException("Only fields and properties can be projected."),
            },
        };

    private static Type? GetEnumerableElementType(Type type)
    {
        if (type == typeof(string))
        {
            return null;
        }

        return new[] { type }.Union(type.GetInterfaces())
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            ?.GetGenericArguments()
            .FirstOrDefault();
    }

    private static string GetProjectionKey(SortedSet<SelectItem> items)
        => string.Join(',', items.Select(x => x.Items.Count == 0 ? x.Name : $"{x.Name}({GetProjectionKey(x.Items)})"));

    private class Projection
    {
        required public MemberInfo SourceMember { get; init; }

        required public ProjectionType MemberType { get; init; }

        required public ICollection<Projection>? InnerProjections { get; init; }
    }
}
