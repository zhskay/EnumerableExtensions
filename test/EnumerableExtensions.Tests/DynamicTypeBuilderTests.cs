using EnumerableExtensions.Internal;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace EnumerableExtensions.Tests;

[ExcludeFromCodeCoverage]
public class DynamicTypeBuilderTests
{
    [Test]
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
            Assert.That(type.Name, Is.EqualTo("Foo"));
            Assert.That(type.BaseType, Is.EqualTo(typeof(TestClass)));
            Assert.That(type.GetFields(), Is.All.Matches<FieldInfo>(fi => fi.Name == $"{fi.FieldType.Name}Field" && types.Contains(fi.FieldType)));
            Assert.That(type.GetProperties(), Is.All.Matches<PropertyInfo>(pi => pi.Name == $"{pi.PropertyType.Name}Property" && types.Contains(pi.PropertyType)));
        });
    }

    public class TestClass
    {
    }
}
