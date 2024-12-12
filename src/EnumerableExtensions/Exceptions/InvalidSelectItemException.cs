using EnumerableExtensions.Internal;

namespace EnumerableExtensions.Exceptions;

/// <summary>
/// Represents an exception thrown when a select item is invalid for a specified type.
/// </summary>
public class InvalidSelectItemException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidSelectItemException"/> class with the invalid select item, target type, and an optional message.
    /// </summary>
    /// <param name="item">The invalid select item.</param>
    /// <param name="type">The type that the select item is invalid for.</param>
    /// <param name="message">An optional message describing the error.</param>
    public InvalidSelectItemException(SelectItem item, Type type, string? message = null)
        : base(GenerateMessage(item, type, message))
    {
        this.SelectItem = item;
        this.Type = type;
    }

    /// <summary>
    /// Gets the type that the select item is invalid for.
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// Gets the invalid select item that caused the exception.
    /// </summary>
    public SelectItem SelectItem { get; }

    private static string GenerateMessage(SelectItem selectItem, Type type, string? message)
        => string.Format(
            "Invalid select member '{0}' of the type {1}.{2}",
            selectItem.Name,
            type.FullName,
            string.IsNullOrWhiteSpace(message) ? string.Empty : " " + message);
}
