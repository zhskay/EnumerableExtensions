using System.Text.Json;

namespace EnumerableExtensions.Tests;

public class SelectPartiallyTests
{
    [Test]
    public void SelectPartially_ListItems_ShouldReturnOnlySelectedFields()
    {
        List<TestClass> items = new()
        {
            new() { Id = 1, Name = "Name", FullName = "FullName", Address = "Address" },
            new() { Id = 2, Name = "Name2", FullName = "FullName2", Address = "Address2" }
        };

        List<object> result = items.AsQueryable().SelectPartially(["Id", "Name"]).ToList();

        string serialized = JsonSerializer.Serialize(result);

        Assert.That(serialized, Is.EqualTo("""[{"Id":1,"Name":"Name"},{"Id":2,"Name":"Name2"}]"""));
    }

    public class TestClass
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string FullName { get; set; }

        public string Address { get; set; }
    }
}