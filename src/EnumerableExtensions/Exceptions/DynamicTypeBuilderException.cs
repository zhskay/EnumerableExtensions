namespace EnumerableExtensions.Exceptions;

/// <summary>
/// Represents an exception thrown during dynamic type creation when type specification is invalid.
/// </summary>
/// <param name="message">The message that describes the error.</param>
public class DynamicTypeBuilderException(string message)
    : Exception(message)
{
}
