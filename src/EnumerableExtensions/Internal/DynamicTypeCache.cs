using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace EnumerableExtensions.Internal;

public class DynamicTypeCache
{
    internal static readonly ConcurrentDictionary<string, Type> BuiltTypes = new();
    internal static readonly ConcurrentDictionary<string, LambdaExpression> Projections = new();

    internal static Func<AssemblyCreatingStrategy> AssemblyCreatingStrategyFactoryFunc { get; private set; }
        = () => AssemblyCreatingStrategy.AssemblyPerHierarchy;

    public static void Clear()
    {
        Projections.Clear();
        BuiltTypes.Clear();
    }
}
