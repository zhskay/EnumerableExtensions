using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
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
        ModuleBuilder = AssemblyBuilder.DefineDynamicAssembly(AssemblyName, AssemblyBuilderAccess.RunAndCollect)
            .DefineDynamicModule(AssemblyName.Name);
    }

    /// <summary>
    /// Returns dynamically created <see cref="Type" /> that contains only specific fields.
    /// </summary>
    /// <param name="dynamicType"> Dynamic type specification. </param>
    /// <returns> <see cref="Type" /> that corresponds a given specification. </returns>
    public static Type GetOrBuildDynamicType(DynamicType dynamicType)
    {
        dynamicType.Validate(nameof(dynamicType));

        string typeKey = GetTypeKey(dynamicType);

        return BuiltTypes.GetOrAdd(typeKey, (_) => BuildDynamicType(dynamicType));
    }

    /// <summary>
    /// Returns dynamically created <see cref="Type" /> that contains only specific fields.
    /// </summary>
    /// <param name="dynamicType"> Dynamic type specification. </param>
    /// <returns> <see cref="Type" /> that corresponds a given specification. </returns>
    public static Type BuildDynamicType(DynamicType dynamicType)
    {
        dynamicType.Validate(nameof(dynamicType));

        string typeName = dynamicType.Name ?? "DynamicType" + BuiltTypes.Count.ToString();

        TypeBuilder typeBuilder = ModuleBuilder.DefineType(
            typeName,
            TypeAttributes.Public | TypeAttributes.Class,
            dynamicType.BaseType,
            Type.EmptyTypes);

        foreach (DynamicTypeMember member in dynamicType.Members)
        {
            switch (member.MemberType)
            {
                case DynamicTypeMemberType.Field:
                    DefineField(typeBuilder, member.Name, member.Type);
                    break;
                case DynamicTypeMemberType.Property:
                    DefineProperty(typeBuilder, member.Name, member.Type);
                    break;
            }
        }

        return typeBuilder.CreateType();
    }

    private static void DefineProperty(TypeBuilder typeBuilder, string name, Type type)
    {
        FieldBuilder fieldBuilder = typeBuilder.DefineField($"_{name}", type, FieldAttributes.Private);
        PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(name, PropertyAttributes.HasDefault, type, null);

        // The property set and property get methods require a special set of attributes.
        MethodAttributes getSetAttr = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;

        // Define the "get" accessor method for property.
        MethodBuilder getPropMethodBuilder = typeBuilder.DefineMethod($"get_{name}", getSetAttr, type, Type.EmptyTypes);

        ILGenerator getIL = getPropMethodBuilder.GetILGenerator();

        getIL.Emit(OpCodes.Ldarg_0);
        getIL.Emit(OpCodes.Ldfld, fieldBuilder);
        getIL.Emit(OpCodes.Ret);

        // Define the "set" accessor method for property.
        MethodBuilder setPropMethodBuilder = typeBuilder.DefineMethod($"set_{name}", getSetAttr, null, [type]);

        ILGenerator setIL = setPropMethodBuilder.GetILGenerator();

        setIL.Emit(OpCodes.Ldarg_0);
        setIL.Emit(OpCodes.Ldarg_1);
        setIL.Emit(OpCodes.Stfld, fieldBuilder);
        setIL.Emit(OpCodes.Ret);

        // Last, we must map the two methods created above to our PropertyBuilder to
        // their corresponding behaviors, "get" and "set" respectively.
        propertyBuilder.SetGetMethod(getPropMethodBuilder);
        propertyBuilder.SetSetMethod(setPropMethodBuilder);
    }

    private static void DefineField(TypeBuilder typeBuilder, string name, Type type)
    {
        CustomAttributeBuilder attributeBuilder = new(
            typeof(JsonIncludeAttribute).GetConstructor(Type.EmptyTypes), []);

        typeBuilder.DefineField(name, type, FieldAttributes.Public)
                .SetCustomAttribute(attributeBuilder);
    }

    private static string GetTypeKey(DynamicType options)
    {
        StringBuilder stringBuilder = new();

        foreach (DynamicTypeMember member in options.Members.OrderBy(m => m.Name).ThenBy(m => m.Type.Name))
        {
            // TODO member type
            stringBuilder.AppendFormat("{0},{1};", member.Name, member.Type.Name);
        }

        return stringBuilder.ToString();
    }
}
