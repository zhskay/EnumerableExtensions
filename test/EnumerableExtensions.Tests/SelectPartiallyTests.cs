using EnumerableExtensions.Internal;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace EnumerableExtensions.Tests;

[ExcludeFromCodeCoverage]
public class SelectPartiallyTests
{
    private static readonly List<TestClass> TestItems = [
        new() { Id = 1, Name = "Name", FullName = "FullName", Address = "Address", NameField = "Name" },
        new() { Id = 2, Name = "Name2", FullName = "FullName2", Address = "Address2", NameField = "Name2" },
    ];

    [Theory]
    [InlineData(new[] { "Id", "Name" }, """[{"Id":1,"Name":"Name"},{"Id":2,"Name":"Name2"}]""")]
    [InlineData(new[] { "Id", "FullName" }, """[{"Id":1,"FullName":"FullName"},{"Id":2,"FullName":"FullName2"}]""")]
    [InlineData(new[] { "Name", "Address" }, """[{"Name":"Name","Address":"Address"},{"Name":"Name2","Address":"Address2"}]""")]
    public void SelectPartially_ItemList_ShouldReturnOnlySelectedFields(ICollection<string> propertyNames, string expected)
    {
        List<object> result = TestItems.AsQueryable().SelectPartially(propertyNames).ToList();

        string serialized = JsonSerializer.Serialize(result);

        Assert.Equal(expected, serialized);
    }

    [Theory]
    [InlineData(new[] { "Id", "Name" }, """[{"Id":1,"Name":"Name"},{"Id":2,"Name":"Name2"}]""")]
    [InlineData(new[] { "Id", "FullName" }, """[{"Id":1,"FullName":"FullName"},{"Id":2,"FullName":"FullName2"}]""")]
    [InlineData(new[] { "Name", "Address" }, """[{"Name":"Name","Address":"Address"},{"Name":"Name2","Address":"Address2"}]""")]
    public void SelectPartially_ItemList_ShouldReturnOnlySelectedProperties(ICollection<string> propertyNames, string expected)
    {
        List<object> result = TestItems.AsQueryable().SelectPartially(propertyNames, new() { DestinationMemberType = DynamicTypeMemberType.Property }).ToList();

        string serialized = JsonSerializer.Serialize(result);

        Assert.Equal(expected, serialized);
    }

    [Theory]
    [InlineData(new[] { "Id", "Name", "NameField" }, """[{"Id":1,"Name":"Name","NameField":"Name"},{"Id":2,"Name":"Name2","NameField":"Name2"}]""")]
    [InlineData(new[] { "Id", "FullName" }, """[{"Id":1,"FullName":"FullName"},{"Id":2,"FullName":"FullName2"}]""")]
    [InlineData(new[] { "Name", "Address" }, """[{"Name":"Name","Address":"Address"},{"Name":"Name2","Address":"Address2"}]""")]
    public void SelectPartially_ItemList_ShouldReturnOnlySelectedMembers(ICollection<string> propertyNames, string expected)
    {
        ProjectionOptions options = new() { MemberTypeAsSource = true };
        List<object> result = TestItems.AsQueryable().SelectPartially(propertyNames, options).ToList();

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
    }
}