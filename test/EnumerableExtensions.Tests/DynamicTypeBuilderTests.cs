using EnumerableExtensions.Internal;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace EnumerableExtensions.Tests;

[ExcludeFromCodeCoverage]
public class DynamicTypeBuilderTests
{
    [Fact]
    public void GetDynamicType_AllKindOfMembers_ShouldReturnDynamicType()
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

        var type = DynamicTypeBuilder.GetOrBuildDynamicType(new DynamicType
        {
            Name = "Foo",
            BaseType = typeof(TestClass),
            Members = types.Select(type => new DynamicTypeMember() { Name = $"{type.Name}Field", Type = type, MemberType = DynamicTypeMemberType.Field, })
                .Union(types.Select(type => new DynamicTypeMember() { Name = $"{type.Name}Property", Type = type, MemberType = DynamicTypeMemberType.Property, }))
                .ToList(),
        });

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
