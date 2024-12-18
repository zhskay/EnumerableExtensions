using EnumerableExtensions.Exceptions;
using EnumerableExtensions.Parsing;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

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

        ParameterExpression sourceItem = Expression.Parameter(typeof(T), "x");

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
        return Expression.Lambda<Func<T, object>>(CreateObjectProjection(dynamicType, projections, sourceItem), sourceItem);
    }

    private static MemberInitExpression CreateObjectProjection(Type type, ICollection<Projection> projections, Expression parameterExpression)
    {
        MemberInfo[] destMembers = type.GetMembers()
            .Where(mi => mi.MemberType is MemberTypes.Property or MemberTypes.Field)
            .ToArray();

        IEnumerable<MemberBinding> bindings = projections
            .Select(projection =>
            {
                MemberInfo destMember = destMembers.FirstOrDefault(mi => mi.Name == projection.SourceMember.Name)
                    ?? throw new Exception($"Destination type does not have member {projection.SourceMember.Name} for project to.");

                MemberExpression sourceMemberExpression = Expression.MakeMemberAccess(parameterExpression, projection.SourceMember);

                if (projection.InnerProjections is null or { Count: 0 })
                {
                    return Expression.Bind(destMember, sourceMemberExpression);
                }

                return GetUnderlyingType(destMember) switch
                {
                    Type arrayType when arrayType.IsArray
                        => Expression.Bind(
                            destMember,
                            Expression.Condition(
                                Expression.Equal(sourceMemberExpression, Expression.Constant(null)),
                                Expression.Constant(null, arrayType),
                                CreateArrayProjection(arrayType, projection.InnerProjections, sourceMemberExpression))),

                    Type classType when classType.IsClass
                        => Expression.Bind(
                            destMember,
                            Expression.Condition(
                                Expression.Equal(sourceMemberExpression, Expression.Constant(null)),
                                Expression.Constant(null, classType),
                                CreateObjectProjection(classType, projection.InnerProjections, sourceMemberExpression))),

                    _ => Expression.Bind(destMember, sourceMemberExpression),
                };
            });

        return Expression.MemberInit(Expression.New(type), bindings);
    }

    private static MethodCallExpression CreateArrayProjection(Type arrayType, ICollection<Projection> projections, Expression parameterExpression)
    {
        if (arrayType.GetElementType() is not Type destType || parameterExpression.Type.GetElementType() is not Type sourceType)
        {
            throw new InvalidOperationException();
        }

        ParameterExpression item = Expression.Parameter(sourceType, "item");

        MethodCallExpression expression = Expression.Call(
            null,
            Helper.SelectMethodInfo.MakeGenericMethod(sourceType, destType),
            parameterExpression,
            Expression.Lambda(CreateObjectProjection(destType, projections, item), item));

        return Expression.Call(
            null,
            Helper.ToArray.MakeGenericMethod(destType),
            expression);
    }

    private static Projection[] CreateProjections(Type type, IEnumerable<SelectItem> selectItems, ProjectionOptions options)
    {
        if (type.IsArray && type.GetElementType() is Type elementType)
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

        return new MemberSpec
        {
            Name = projection.SourceMember.Name,
            MemberType = GetMemberType(projection),
            IsArray = sourceMemberType.IsArray,
            Type = projection.InnerProjections is null
                ? sourceMemberType.IsArray ? sourceMemberType.GetElementType() : sourceMemberType
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

    private static string GetProjectionKey(SortedSet<SelectItem> items)
        => string.Join(',', items.Select(x => x.Items.Count == 0 ? x.Name : $"{x.Name}({GetProjectionKey(x.Items)})"));

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = nameof(Enumerable.Select))]
    private static extern IEnumerable<TResult> SelectMethod<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, TResult> selector);

    private class Projection
    {
        required public MemberInfo SourceMember { get; init; }

        required public ProjectionType MemberType { get; init; }

        required public ICollection<Projection>? InnerProjections { get; init; }
    }

    private static class Helper
    {
        static Helper()
        {
            ToArray = typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray));

            SelectMethodInfo = typeof(Enumerable).GetMethods()
                .FirstOrDefault(m =>
                    m.Name == nameof(Enumerable.Select)
                    && m.GetParameters().All(p => p.ParameterType.IsGenericType)
                    && m.GetParameters().Select(p => p.ParameterType.GetGenericTypeDefinition()).SequenceEqual([typeof(IEnumerable<>), typeof(Func<,>)]));
        }

        public static MethodInfo SelectMethodInfo { get; }

        public static MethodInfo ToArray { get; }
    }
}
