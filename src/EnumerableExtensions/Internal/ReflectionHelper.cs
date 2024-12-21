using System.Reflection;

namespace EnumerableExtensions.Internal;

/// <summary>
/// Provides cached access to reflection-based methods to improve performance by avoiding repeated lookups.
/// </summary>
internal static class ReflectionHelper
{
    private static readonly Lazy<MethodInfo> LazySelectMethodInfo = new(() =>
    {
        return typeof(Enumerable).GetMethods()
            .FirstOrDefault(m =>
                m.Name == nameof(Enumerable.Select)
                && m.GetParameters().All(p => p.ParameterType.IsGenericType)
                && m.GetParameters().Select(p => p.ParameterType.GetGenericTypeDefinition())
                   .SequenceEqual([typeof(IEnumerable<>), typeof(Func<,>)]))
            ?? throw new InvalidOperationException("Could not find Select method.");
    });

    private static readonly Lazy<MethodInfo> LazyToArrayMethodInfo = new(() =>
        typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray))
        ?? throw new InvalidOperationException("Could not find ToArray method."));

    /// <summary>
    /// Gets the <see cref="MethodInfo"/> for the <see cref="Enumerable.Select{TSource, TResult}(IEnumerable{TSource}, Func{TSource, TResult})"/> method.
    /// </summary>
    public static MethodInfo SelectMethodInfo => LazySelectMethodInfo.Value;

    /// <summary>
    /// Gets the <see cref="MethodInfo"/> for the <see cref="Enumerable.ToArray{TSource}(IEnumerable{TSource})"/> method.
    /// </summary>
    public static MethodInfo ToArray => LazyToArrayMethodInfo.Value;
}
