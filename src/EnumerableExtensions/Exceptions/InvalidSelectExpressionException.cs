namespace EnumerableExtensions.Exceptions;

/// <summary>
/// Represents an exception thrown when a select expression is invalid.
/// </summary>
public class InvalidSelectExpressionException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidSelectExpressionException"/> class with the specified select expression, position, and an optional message.
    /// </summary>
    /// <param name="select">The invalid select expression.</param>
    /// <param name="position">The position in the select expression where the error occurred.</param>
    /// <param name="message">An optional error message providing additional context.</param>
    public InvalidSelectExpressionException(string select, int position, string? message = null)
        : base(GenerateMessage(select, position, message))
    {
        this.Select = select;
        this.Position = position;
    }

    /// <summary>
    /// Gets the invalid select expression that caused the exception.
    /// </summary>
    public string Select { get; }

    /// <summary>
    /// Gets the position in the select expression where the error occurred.
    /// </summary>
    public int Position { get; }

    private static string GenerateMessage(string select, int position, string? message)
        => string.Format("Invalid select expression at position {0}. Select: {1}.{2}", position, select, string.IsNullOrWhiteSpace(message) ? string.Empty : " " + message);
}
