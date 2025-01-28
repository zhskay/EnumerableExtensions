using System.Reflection;
using System.Reflection.Emit;

namespace EnumerableExtensions.Internal;

public abstract class AssemblyCreatingStrategy
{
    public static AssemblyCreatingStrategy AssemblyPerType => new AssemblyPerTypeStrategy();

    public static AssemblyCreatingStrategy AssemblyPerHierarchy => new AssemblyPerHierarchyStrategy();

    public static AssemblyCreatingStrategy OneAssembly => new OneAssemblyForAllTypesStrategy();

    public abstract ModuleBuilder GetModuleBuilder(string typeName);

    public class AssemblyPerTypeStrategy : AssemblyCreatingStrategy
    {
        public override ModuleBuilder GetModuleBuilder(string typeName)
        {
            AssemblyName assemblyName = new() { Name = $"DynamicAssemblyFor{typeName}" };

            return AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect)
                .DefineDynamicModule(assemblyName.Name);
        }
    }

    public class AssemblyPerHierarchyStrategy : AssemblyCreatingStrategy
    {
        private ModuleBuilder? moduleBuilder;

        public override ModuleBuilder GetModuleBuilder(string typeName)
        {
            if (this.moduleBuilder is null)
            {
                AssemblyName assemblyName = new() { Name = $"DynamicAssemblyFor{typeName}" };
                this.moduleBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect)
                    .DefineDynamicModule(assemblyName.Name);
            }

            return this.moduleBuilder;
        }
    }

    public class OneAssemblyForAllTypesStrategy : AssemblyCreatingStrategy
    {
        private static readonly Lazy<ModuleBuilder> ModuleBuilder = new Lazy<ModuleBuilder>(() =>
        {
            AssemblyName assemblyName = new() { Name = "DynamicAssemblyForAllDynamicTypes" };
            return AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect)
                .DefineDynamicModule(assemblyName.Name);
        });

        public override ModuleBuilder GetModuleBuilder(string typeName)
        {
            return ModuleBuilder.Value;
        }
    }
}
