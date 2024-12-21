using EnumerableExtensions.Exceptions;
using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json.Serialization;

namespace EnumerableExtensions.Internal;

/// <summary>
/// Provides functionality for creating dynamically generated types based on specifications.
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
    /// Returns dynamically created <see cref="Type" /> based on the specification.
    /// </summary>
    /// <param name="typeSpec">The specification for the dynamic type.</param>
    /// <returns>A <see cref="Type" /> that corresponds to the given specification.</returns>
    public static Type GetOrCreateDynamicType(TypeSpec typeSpec)
    {
        string typeKey = GetTypeKey(typeSpec);

        return BuiltTypes.GetOrAdd(typeKey, (_) => CreateDynamicType(typeSpec));
    }

    /// <summary>
    /// Creates a new dynamically generated <see cref="Type" /> based on the specification.
    /// </summary>
    /// <param name="typeSpec">The specification for the dynamic type.</param>
    /// <returns>A <see cref="Type" /> that corresponds to the given specification.</returns>
    public static Type CreateDynamicType(TypeSpec typeSpec)
    {
        string typeName = typeSpec.Name ?? "DynamicType" + typeSpec.GetHashCode();

        TypeBuilder typeBuilder = ModuleBuilder.DefineType(
            typeName,
            TypeAttributes.Public | TypeAttributes.Class,
            typeSpec.BaseType,
            Type.EmptyTypes);

        foreach (MemberSpec memberSpec in typeSpec.Members)
        {
            Type type = GetTypeOfMember(memberSpec);

            switch (memberSpec.MemberType)
            {
                case MemberTypes.Field:
                    DefineField(typeBuilder, memberSpec.Name, type);
                    break;
                case MemberTypes.Property:
                    DefineProperty(typeBuilder, memberSpec.Name, type);
                    break;
                default:
                    throw new DynamicTypeBuilderException("Only fields and properties can be created.");
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
        MethodBuilder setPropMethodBuilder = typeBuilder.DefineMethod($"set_{name}", getSetAttr, null, new[] { type });

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
            typeof(JsonIncludeAttribute).GetConstructor(Type.EmptyTypes), Array.Empty<object>());

        typeBuilder.DefineField(name, type, FieldAttributes.Public)
                .SetCustomAttribute(attributeBuilder);
    }

    private static Type GetTypeOfMember(MemberSpec member)
    {
        Type type = member.Type is not null
            ? member.Type
            : member.TypeSpec is not null
                ? GetOrCreateDynamicType(member.TypeSpec)
                : throw new DynamicTypeBuilderException("Specify Type or TypeSpec");

        return member.IsEnumerable
            ? typeof(IEnumerable<>).MakeGenericType(type)
            : type;
    }

    private static string GetTypeKey(TypeSpec? spec)
    {
        if (spec is null)
        {
            return string.Empty;
        }

        StringBuilder stringBuilder = new();

        foreach (MemberSpec member in spec.Members.OrderBy(m => m.Name))
        {
            stringBuilder.AppendFormat("{0},{1};", member.Name, member.Type?.Name ?? GetTypeKey(member.TypeSpec));
        }

        return stringBuilder.ToString();
    }
}
