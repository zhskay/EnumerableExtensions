using EnumerableExtensions.Internal;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EnumerableExtensions.Tests;

[ExcludeFromCodeCoverage]
public class SelectPartiallyTests
{
    private static readonly List<TestClass> TestItems = [
        new()
        {
            Id = 1,
            Name = "Name",
            FullName = "FullName",
            Address = "Address",
            NameField = "Name",
            Inner = new() { Id = 3, Name = "InnerName" },
            InnerArray = [new() { Id = 4, Name = "ArrayItem" }, new() { Id = 5, Name = "ArrayItem2" }],
            InnerList = [new() { Id = 6, Name = "ListItem" }, new() { Id = 7, Name = "ListItem2" }],
        },
        new()
        {
            Id = 2,
            Name = "Name2",
            FullName = "FullName2",
            Address = "Address2",
            NameField = "Name2",
        },
    ];

    [Theory]

    // field projection
    [InlineData("Id,Name", ProjectionType.Field, """[{"Id":1,"Name":"Name"},{"Id":2,"Name":"Name2"}]""")]
    [InlineData("Id,FullName", ProjectionType.Field, """[{"Id":1,"FullName":"FullName"},{"Id":2,"FullName":"FullName2"}]""")]
    [InlineData("Name,Address", ProjectionType.Field, """[{"Name":"Name","Address":"Address"},{"Name":"Name2","Address":"Address2"}]""")]

    // property projection
    [InlineData("Id,Name", ProjectionType.Property, """[{"Id":1,"Name":"Name"},{"Id":2,"Name":"Name2"}]""")]
    [InlineData("Id,FullName", ProjectionType.Property, """[{"Id":1,"FullName":"FullName"},{"Id":2,"FullName":"FullName2"}]""")]
    [InlineData("Name,Address", ProjectionType.Property, """[{"Name":"Name","Address":"Address"},{"Name":"Name2","Address":"Address2"}]""")]

    // as source
    [InlineData("Id,Name", ProjectionType.AsSource, """[{"Id":1,"Name":"Name"},{"Id":2,"Name":"Name2"}]""")]
    [InlineData("Id,FullName", ProjectionType.AsSource, """[{"Id":1,"FullName":"FullName"},{"Id":2,"FullName":"FullName2"}]""")]
    [InlineData("Name,Address", ProjectionType.AsSource, """[{"Name":"Name","Address":"Address"},{"Name":"Name2","Address":"Address2"}]""")]

    // inner objects
    [InlineData("Id,Inner", ProjectionType.Field, """[{"Id":1,"Inner":{"Id":3,"Name":"InnerName"}},{"Id":2}]""")]
    [InlineData("Id,Name,Inner(Id)", ProjectionType.Field, """[{"Id":1,"Name":"Name","Inner":{"Id":3}},{"Id":2,"Name":"Name2"}]""")]

    // inner array
    [InlineData("Id,InnerArray", ProjectionType.Field, """[{"Id":1,"InnerArray":[{"Id":4,"Name":"ArrayItem"},{"Id":5,"Name":"ArrayItem2"}]},{"Id":2}]""")]
    [InlineData("Id,InnerArray(Id)", ProjectionType.Field, """[{"Id":1,"InnerArray":[{"Id":4},{"Id":5}]},{"Id":2}]""")]

    // inner list
    // [InlineData("Id,InnerList", ProjectionType.Field, """[{"Id":1,"InnerList":[{"Id":6,"Name":"ListItem"},{"Id":7,"Name":"ListItem2"}]},{"Id":2}]""")]
    // [InlineData("Id,InnerList(Id,Name)", ProjectionType.Field, """[{"Name":"Name","Address":"Address"},{"Name":"Name2","Address":"Address2"}]""")]
    public void SelectPartially_ItemList_ShouldReturnOnlySelectedMembers(string select, ProjectionType type, string expected)
    {
        List<object> result = TestItems.AsQueryable().SelectPartially(select, new() { MemberType = type }).ToList();

        string serialized = JsonSerializer.Serialize(result, options: new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });

        Assert.Equal(expected, serialized);
    }

    public class TestClass
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string FullName { get; set; }

        public string Address { get; set; }

        public string NameField;

        public TestClass Inner { get; set; }

        public TestClass[] InnerArray { get; set; }

        public List<TestClass> InnerList { get; set; }
    }
}