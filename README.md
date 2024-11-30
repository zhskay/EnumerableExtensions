# EnumerableExtensions

**EnumerableExtensions** is a C# library that extends the functionality of `IQueryable<T>` with additional utility methods. These extensions make it easier to work with LINQ queries and project specific fields dynamically.

## Features

- **Dynamic Field Selection**: Use `SelectPartially` to project each element of a sequence into a new form containing only the specified fields.
- Provides a clean and reusable way to dynamically shape query results.

## Installation

1. Clone the repository:  
   ```bash
   git clone https://github.com/zhskay/EnumerableExtensions.git
   ```
2. Add the project to your solution or include it as a reference.

## Usage

Here’s an example of how to use `SelectPartially`:

```csharp
var data = context.Users.AsQueryable();
var projectedData = data.SelectPartially(new[] { "Name", "Email" });
```

This will return an `IQueryable<object>` containing only the `Name` and `Email` fields.

## How It Works

The `SelectPartially` method leverages the **ProjectionBuilder** to generate and cache dynamic LINQ expressions. Here's how:

1. **Dynamic Expression Generation**:  
   - The `ProjectionBuilder.Build` method creates an `Expression<Func<T, object>>` based on the specified fields.  
   - A cache ensures efficient reuse of generated expressions for the same set of fields.

2. **Dynamic Type Creation**:  
   - The `DynamicTypeBuilder` generates runtime-defined types with only the required fields.  
   - Each field is annotated for JSON serialization, making the projected data serialization-friendly.

This approach avoids hardcoding field projections, supports dynamic scenarios, and optimizes performance through caching and reuse.

## Thanks

Special thanks to [Ethan J. Brown](https://stackoverflow.com/questions/606104/how-to-create-linq-expression-tree-with-anonymous-type-in-it/723018#723018) for the foundational approach to dynamically generating types, which inspired the implementation of `DynamicTypeBuilder`.

## Contributing

Contributions are welcome! Please fork the repository and submit a pull request.

## License

This project is licensed under the [MIT License](LICENSE).

## Contact

For feedback or issues, open an issue on the GitHub repository.
