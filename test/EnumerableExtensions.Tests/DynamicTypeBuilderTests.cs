﻿using EnumerableExtensions.Internal;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace EnumerableExtensions.Tests;

[ExcludeFromCodeCoverage]
public class DynamicTypeBuilderTests
{
    [Fact]
    public void GetOrCreateDynamicType_TypeWithPrimitiveMembers_ShouldReturnDynamicType()
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

        var specification = new TypeSpec
        {
            Name = "Foo",
            BaseType = typeof(TestClass),
            Members = types.Select(type => new MemberSpec() { Name = $"{type.Name}Field", Type = type, MemberType = MemberTypes.Field, })
                .Union(types.Select(type => new MemberSpec() { Name = $"{type.Name}Property", Type = type, MemberType = MemberTypes.Property, }))
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

    [Fact]
    public void GetOrCreateDynamicType_TypeWithInnerType_ShouldReturnDynamicType()
    {
        var specification = new TypeSpec
        {
            Name = "Bar",
            BaseType = typeof(TestClass),
            Members =
            [
                new MemberSpec
                {
                    Name = "InnerObject",
                    MemberType = MemberTypes.Property,
                    TypeSpec = new TypeSpec
                    {
                        Name = "InnerType",
                        Members =
                        [
                            new MemberSpec
                            {
                                Name = "DeeplyInnerObject",
                                MemberType = MemberTypes.Property,
                                TypeSpec = new TypeSpec
                                {
                                    Name = "DeeplyInnerType",
                                    Members = [new MemberSpec { Name = "String", MemberType = MemberTypes.Property, Type = typeof(string) }],
                                },
                            },
                        ],
                    },
                },
            ],
        };
        var type = DynamicTypeBuilder.GetOrCreateDynamicType(specification);

        Assert.Multiple(() =>
        {
            Assert.Equal("Bar", type.Name);
            Assert.Equal(typeof(TestClass), type.BaseType);

            PropertyInfo? innerProperty = type.GetProperty("InnerObject");

            Assert.NotNull(innerProperty);
            Assert.Equal("InnerObject", innerProperty?.Name);
            Assert.Equal("InnerType", innerProperty?.PropertyType.Name);

            PropertyInfo? deelpyInnerProperty = innerProperty?.PropertyType?.GetProperty("DeeplyInnerObject");

            Assert.NotNull(deelpyInnerProperty);
            Assert.Equal("DeeplyInnerObject", deelpyInnerProperty?.Name);
            Assert.Equal("DeeplyInnerType", deelpyInnerProperty?.PropertyType.Name);
        });
    }

    public class TestClass
    {
    }
}
