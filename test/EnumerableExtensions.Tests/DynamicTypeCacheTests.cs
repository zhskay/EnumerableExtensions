using EnumerableExtensions.Internal;
using System.Diagnostics.CodeAnalysis;

namespace EnumerableExtensions.Tests;

[ExcludeFromCodeCoverage]
[CollectionDefinition("Non-Parallel Collection", DisableParallelization = true)]
public class DynamicTypeCacheTests
{
    [Fact]
    public void DynamicTypeCache_CreateType_DynamicAssemblyUnloaded()
    {
        DynamicTypeCache.Clear();

        this.CreateType();

        DynamicTypeCache.Clear();

        GC.Collect();
        GC.WaitForPendingFinalizers();

        var dynamicAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName.StartsWith("DynamicAssemblyForFoo"));
        Assert.Null(dynamicAssembly);
    }

    [Fact]
    public void DynamicTypeCache_CreateProjection_DynamicAssemblyUnloaded()
    {
        DynamicTypeCache.Clear();

        this.CreateProjection();

        DynamicTypeCache.Clear();

        GC.Collect();
        GC.WaitForPendingFinalizers();

        var dynamicAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName.StartsWith("DynamicAssemblyForDynamicType"));
        Assert.Null(dynamicAssembly);
    }

    private void CreateType()
    {
        TypeSpec specification = new()
        {
            Name = "Foo",
            Members = [],
        };
        Type type = DynamicTypeBuilder.GetOrCreateDynamicType(specification);

        var dynamicAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName.StartsWith("DynamicAssemblyForFoo"));
        Assert.NotNull(dynamicAssembly);
    }

    private void CreateProjection()
    {
        var dynamicAssemblys = AppDomain.CurrentDomain.GetAssemblies();

        var items = new[]
        {
            new TestClass { Id = 1, Name = "First" },
            new TestClass { Id = 2, Name = "Second" },
        };

        var newItems = items.AsQueryable().SelectPartially("Id");

        var dynamicAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName.StartsWith("DynamicAssemblyFor"));
        Assert.NotNull(dynamicAssembly);
    }

    private class TestClass
    {
        public int Id { get; init; }

        public string Name { get; init; }
    }
}
