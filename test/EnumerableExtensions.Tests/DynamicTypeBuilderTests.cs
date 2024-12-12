using EnumerableExtensions.Internal;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace EnumerableExtensions.Tests;

[ExcludeFromCodeCoverage]
public class DynamicTypeBuilderTests
{
    [Fact]
    public void GetDynamicType_PrimitiveMembers_ShouldReturnDynamicType()
    {
        Type[] types = [
            typeof(bool),
            typeof(byte),
            typeof(sbyte),
            typeof(char),
            typeof(decimal),
            typeof(double),
            typeof(float),
            typeof(int),
            typeof(uint),
            typeof(nint),
            typeof(nuint),
            typeof(long),
            typeof(ulong),
            typeof(short),
            typeof(ushort),
            typeof(string),
        ];

        var specification = new DynamicType
        {
            Name = "Foo",
            BaseType = typeof(TestClass),
            Members = types.Select(type => new DynamicTypeMember() { Name = $"{type.Name}Field", Type = type, MemberType = MemberTypes.Field, })
                .Union(types.Select(type => new DynamicTypeMember() { Name = $"{type.Name}Property", Type = type, MemberType = MemberTypes.Property, }))
                .ToList(),
        };
        var type = DynamicTypeBuilder.GetOrCreateDynamicType(specification);

        Assert.Multiple(() =>
        {
            Assert.Equal("Foo", type.Name);
            Assert.Equal(typeof(TestClass), type.BaseType);
            Assert.All(type.GetFields(), fi =>
            {
                Assert.Equal($"{fi.FieldType.Name}Field", fi.Name);
                Assert.Contains(fi.FieldType, types);
            });
            Assert.All(type.GetProperties(), pi =>
            {
                Assert.Equal($"{pi.PropertyType.Name}Property", pi.Name);
                Assert.Contains(pi.PropertyType, types);
            });
        });
    }

    public class TestClass
    {
    }
}
