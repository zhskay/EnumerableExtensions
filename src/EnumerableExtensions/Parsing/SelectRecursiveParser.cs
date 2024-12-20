﻿using EnumerableExtensions.Exceptions;

namespace EnumerableExtensions.Parsing;

/// <summary>
/// A static class that provides functionality to recursively parse a select expression string into a collection of <see cref="SelectItem"/> objects.
/// </summary>
/// <remarks>
/// This class uses recursion to handle parsing of select expressions. The expression can contain fields separated by commas,
/// and support for nested items enclosed in parentheses is handled by recursive calls. The parsing algorithm processes each item
/// by checking for commas, parentheses, and field names, ensuring that invalid characters or unexpected expressions are flagged
/// as errors. The algorithm ensures that the expression is correctly structured and throws appropriate exceptions for malformed input.
/// </remarks>
public static class SelectRecursiveParser
{
    private const char Comma = ',';
    private const char OpenParenthesis = '(';
    private const char CloseParenthesis = ')';

    /// <summary>
    /// Parses a select expression into a sorted set of <see cref="SelectItem"/> objects.
    /// </summary>
    /// <param name="select">The select expression to parse, which may include field names and nested items in parentheses.</param>
    /// <returns>A sorted set of <see cref="SelectItem"/> objects representing the parsed select expression.</returns>
    /// <exception cref="InvalidSelectExpressionException">Thrown if the select expression is invalid.</exception>
    public static SortedSet<SelectItem> ParseSelect(string select)
    {
        if (string.IsNullOrWhiteSpace(select))
        {
            throw new InvalidSelectExpressionException(select, 0, "Select expression cannot be empty.");
        }

        int index = 0;
        SortedSet<SelectItem> result = ParseItems(select, ref index);

        if (index != select.Length)
        {
            throw new InvalidSelectExpressionException(select, index, "Unexpected characters at the end of the expression.");
        }

        return result;
    }

    /// <summary>
    /// Recursively parses a list of items in the select expression.
    /// </summary>
    /// <param name="select">The select expression to parse.</param>
    /// <param name="index">The current index in the string during parsing.</param>
    /// <returns>A sorted set of <see cref="SelectItem"/> objects.</returns>
    private static SortedSet<SelectItem> ParseItems(string select, ref int index)
    {
        int order = 0;
        SortedSet<SelectItem> items = new(new SelectItemComparer(StringComparer.OrdinalIgnoreCase));

        while (index < select.Length)
        {
            SkipWhitespace(select, ref index);

            SelectItem item = ParseItem(select, ref index, ref order);
            items.Add(item);

            SkipWhitespace(select, ref index);

            if (index < select.Length)
            {
                if (select[index] == Comma)
                {
                    index++; // Skip the comma
                }
                else if (select[index] == CloseParenthesis)
                {
                    break;
                }
                else
                {
                    throw new InvalidSelectExpressionException(select, index, "Unexpected character after item.");
                }
            }
        }

        if (items.Count == 0)
        {
            throw new InvalidSelectExpressionException(select, index, "No items found.");
        }

        return items;
    }

    /// <summary>
    /// Parses a single item (field or parenthesized group) from the select expression.
    /// </summary>
    /// <param name="select">The select expression to parse.</param>
    /// <param name="index">The current index in the string during parsing.</param>
    /// <returns>A <see cref="SelectItem"/> representing the parsed item.</returns>
    private static SelectItem ParseItem(string select, ref int index, ref int order)
    {
        int startIndex = index;

        while (index < select.Length && select[index] != Comma && select[index] != OpenParenthesis && select[index] != CloseParenthesis)
        {
            if (!char.IsLetterOrDigit(select[index]) && select[index] != '_')
            {
                throw new InvalidSelectExpressionException(select, index, "Invalid character in field name.");
            }

            index++;
        }

        if (startIndex == index)
        {
            throw new InvalidSelectExpressionException(select, index, "Expected a field name.");
        }

        if (char.IsDigit(select[startIndex]))
        {
            throw new InvalidSelectExpressionException(select, index, "Field name can't starts with a digit.");
        }

        string name = select[startIndex..index];

        if (index < select.Length && select[index] == OpenParenthesis)
        {
            index++; // Skip the '('

            SortedSet<SelectItem> innerItems = ParseItems(select, ref index);

            if (index >= select.Length || select[index] != CloseParenthesis)
            {
                throw new InvalidSelectExpressionException(select, index, "Expected closing parenthesis.");
            }

            index++; // Skip the ')'

            return new SelectItem(name, order++, innerItems);
        }

        return new SelectItem(name, order++);
    }

    /// <summary>
    /// Skips whitespace characters in the select expression.
    /// </summary>
    /// <param name="select">The select expression to parse.</param>
    /// <param name="index">The current index in the string during parsing.</param>
    private static void SkipWhitespace(string select, ref int index)
    {
        while (index < select.Length && char.IsWhiteSpace(select[index]))
        {
            index++;
        }
    }
}
