using EnumerableExtensions.Internal;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace EnumerableExtensions.Tests;

[ExcludeFromCodeCoverage]
public class SelectPartiallyTests
{
    private static readonly List<TestClass> TestItems = [
        new() { Id = 1, Name = "Name", FullName = "FullName", Address = "Address", NameField = "Name", Inner = new() { Id = 3, Name = "InnerName" } },
        new() { Id = 2, Name = "Name2", FullName = "FullName2", Address = "Address2", NameField = "Name2" },
    ];

    [Theory]
    [InlineData("Id,Name,Inner(Id,Name)", """[{"Id":1,"Name":"Name","Inner":{"Id":3,"Name":"InnerName"}},{"Id":2,"Name":"Name2","Inner":null}]""")]
    [InlineData("Id,FullName", """[{"Id":1,"FullName":"FullName"},{"Id":2,"FullName":"FullName2"}]""")]
    [InlineData("Name,Address", """[{"Name":"Name","Address":"Address"},{"Name":"Name2","Address":"Address2"}]""")]
    public void SelectPartially_ItemList_ShouldReturnOnlySelectedFields(string select, string expected)
    {
        List<object> result = TestItems.AsQueryable().SelectPartially(select).ToList();

        string serialized = JsonSerializer.Serialize(result);

        Assert.Equal(expected, serialized);
    }

    [Theory]
    [InlineData("Id,Name", """[{"Id":1,"Name":"Name"},{"Id":2,"Name":"Name2"}]""")]
    [InlineData("Id,FullName", """[{"Id":1,"FullName":"FullName"},{"Id":2,"FullName":"FullName2"}]""")]
    [InlineData("Name,Address", """[{"Name":"Name","Address":"Address"},{"Name":"Name2","Address":"Address2"}]""")]
    public void SelectPartially_ItemList_ShouldReturnOnlySelectedProperties(string select, string expected)
    {
        List<object> result = TestItems.AsQueryable().SelectPartially(select, new() { MemberType = ProjectionType.Property }).ToList();

        string serialized = JsonSerializer.Serialize(result);

        Assert.Equal(expected, serialized);
    }

    [Theory]
    [InlineData("Id,Name, NameField", """[{"Id":1,"Name":"Name","NameField":"Name"},{"Id":2,"Name":"Name2","NameField":"Name2"}]""")]
    [InlineData("Id,FullName", """[{"Id":1,"FullName":"FullName"},{"Id":2,"FullName":"FullName2"}]""")]
    [InlineData("Name,Address", """[{"Name":"Name","Address":"Address"},{"Name":"Name2","Address":"Address2"}]""")]
    public void SelectPartially_ItemList_ShouldReturnOnlySelectedMembers(string select, string expected)
    {
        ProjectionOptions options = new() { MemberType = ProjectionType.AsSource };
        List<object> result = TestItems.AsQueryable().SelectPartially(select, options).ToList();

        string serialized = JsonSerializer.Serialize(result);

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
    }
}