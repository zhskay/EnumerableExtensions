using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.Json.Serialization;

namespace EnumerableExtensions.Internal;

/// <summary>
/// Generates <see cref="Type" /> from input parameters.
/// </summary>
/// <remarks>
/// Thanks to Ethan J.Brown: http://stackoverflow.com/questions/606104/how-to-create-linq-expression-tree-with-anonymous-type-in-it/723018#723018.
/// </remarks>
public static class DynamicTypeBuilder
{
    private static readonly AssemblyName AssemblyName = new() { Name = "DynamicLinqTypes" };
    private static readonly ModuleBuilder ModuleBuilder;

    private static readonly ConcurrentDictionary<string, Type> BuiltTypes = new();

    static DynamicTypeBuilder()
    {
        ModuleBuilder = AssemblyBuilder.DefineDynamicAssembly(AssemblyName, AssemblyBuilderAccess.Run)
            .DefineDynamicModule(AssemblyName.Name);
    }

    /// <summary>
    /// Returns dynamically created <see cref="Type" /> that contains only specific fields.
    /// </summary>
    /// <param name="fields"> List of fields that returning type should contain. </param>
    /// <param name="baseType"> Base type of returning type. </param>
    /// <returns> <see cref="Type" /> that contains only specified fields. </returns>
    public static Type GetDynamicType(Dictionary<string, Type> fields, Type? baseType = null)
    {
        ArgumentNullException.ThrowIfNull(fields);
        ArgumentOutOfRangeException.ThrowIfZero(fields.Count);

        string typeKey = GetTypeKey(fields);

        return BuiltTypes.GetOrAdd(typeKey, (_) => BuildDynamicType(fields, baseType));
    }

    private static Type BuildDynamicType(Dictionary<string, Type> fields, Type? baseType)
    {
        string typeName = "DynamicLinqType" + BuiltTypes.Count.ToString();

        TypeBuilder typeBuilder = ModuleBuilder.DefineType(
            typeName,
            TypeAttributes.Public | TypeAttributes.Class,
            baseType,
            Type.EmptyTypes);

        CustomAttributeBuilder attributeBuilder = new(
            typeof(JsonIncludeAttribute).GetConstructor(Type.EmptyTypes), []);

        foreach (var (name, type) in fields)
        {
            typeBuilder.DefineField(name, type, FieldAttributes.Public)
                .SetCustomAttribute(attributeBuilder);
        }

        return typeBuilder.CreateType();
    }

    private static string GetTypeKey(Dictionary<string, Type> fields)
        => string.Join(';', fields.OrderBy(kv => kv.Key).ThenBy(kv => kv.Value.Name).Select(kv => $"{kv.Key},{kv.Value.Name}"));
}
