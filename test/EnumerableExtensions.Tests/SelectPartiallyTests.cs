using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace EnumerableExtensions.Tests;

[ExcludeFromCodeCoverage]
public class SelectPartiallyTests
{
    [TestCase(new[] { "Id", "Name" }, """[{"Id":1,"Name":"Name"},{"Id":2,"Name":"Name2"}]""")]
    [TestCase(new[] { "Id", "FullName" }, """[{"Id":1,"FullName":"FullName"},{"Id":2,"FullName":"FullName2"}]""")]
    [TestCase(new[] { "Name", "Address" }, """[{"Name":"Name","Address":"Address"},{"Name":"Name2","Address":"Address2"}]""")]
    public void SelectPartially_ListItems_ShouldReturnOnlySelectedFields(ICollection<string> propertyNames, string expected)
    {
        List<TestClass> items = new()
        {
            new() { Id = 1, Name = "Name", FullName = "FullName", Address = "Address" },
            new() { Id = 2, Name = "Name2", FullName = "FullName2", Address = "Address2" }
        };

        List<object> result = items.AsQueryable().SelectPartially(propertyNames).ToList();

        string serialized = JsonSerializer.Serialize(result);

        Assert.That(serialized, Is.EqualTo(expected));
    }

    public class TestClass
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string FullName { get; set; }

        public string Address { get; set; }
    }
}