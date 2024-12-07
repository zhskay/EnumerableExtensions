using EnumerableExtensions.Exceptions;

namespace EnumerableExtensions.Internal;

/// <summary>
/// A static class that provides functionality to parse a select expression string into a collection of <see cref="SelectItem"/> objects.
/// </summary>
/// <remarks>
/// This class implements a parsing algorithm that processes a select expression string and converts it into a structured hierarchy of <see cref="SelectItem"/> objects.
/// The algorithm uses a stack to handle nested expressions enclosed in parentheses. It processes the input string character by character, handling commas to separate items,
/// and parentheses to manage nested items. If the expression is malformed, such as an unexpected comma or parentheses, an exception is thrown.
/// The parsed <see cref="SelectItem"/> objects are returned in a sorted collection to maintain a consistent order.
/// </remarks>
public static partial class SelectParser
{
    private const char Comma = ',';
    private const char OpenParenthesis = '(';
    private const char CloseParenthesis = ')';

    /// <summary>
    /// Parses a select expression into a sorted set of <see cref="SelectItem"/> objects.
    /// </summary>
    /// <param name="select">The select expression to parse, which can include field names and nested items in parentheses.</param>
    /// <returns>A sorted set of <see cref="SelectItem"/> objects representing the parsed select expression.</returns>
    /// <exception cref="InvalidSelectExpressionException">Thrown if the select expression is invalid.</exception>
    public static SortedSet<SelectItem> ParseSelect(string select)
    {
        if (string.IsNullOrWhiteSpace(select))
        {
            throw new InvalidSelectExpressionException(select, 0, "Select expression could not be empty.");
        }

        if (select[0] is Comma or OpenParenthesis or CloseParenthesis)
        {
            throw new InvalidSelectExpressionException(select, 0);
        }

        int previousItemStartIndex = 0;
        Stack<SelectItem> stack = [];
        SortedSet<SelectItem> items = new(new SelectItemComparer(StringComparer.OrdinalIgnoreCase));

        for (int i = 0; i < select.Length; i++)
        {
            switch (select[i])
            {
                case OpenParenthesis:
                    if (previousItemStartIndex == i)
                    {
                        throw new InvalidSelectExpressionException(select, i);
                    }

                    SelectItem newParent = new(select[previousItemStartIndex..i]);
                    stack.Push(newParent);

                    previousItemStartIndex = i + 1;

                    break;
                case CloseParenthesis:
                    if (stack.TryPop(out var parent))
                    {
                        if (previousItemStartIndex == i)
                        {
                            AddItem(parent, items, stack);
                        }
                        else
                        {
                            SelectItem newItem = new(select[previousItemStartIndex..i]);
                            parent.Items.Add(newItem);

                            AddItem(parent, items, stack);
                        }
                    }
                    else
                    {
                        throw new InvalidSelectExpressionException(select, i);
                    }

                    previousItemStartIndex = i + 1;

                    break;
                case Comma:
                    if (previousItemStartIndex != i)
                    {
                        SelectItem newItem = new(select[previousItemStartIndex..i]);
                        AddItem(newItem, items, stack);
                    }

                    previousItemStartIndex = i + 1;

                    break;
                case char letter when !char.IsLetter(letter):
                    throw new InvalidSelectExpressionException(select, i + 1);
            }
        }

        if (previousItemStartIndex != select.Length)
        {
            SelectItem newItem = new(select[previousItemStartIndex..]);

            AddItem(newItem, items, stack);
        }

        return items;
    }

    /// <summary>
    /// Adds a <see cref="SelectItem"/> to the appropriate parent or the root set.
    /// </summary>
    /// <param name="item">The <see cref="SelectItem"/> to add.</param>
    /// <param name="items">The collection of root-level items.</param>
    /// <param name="stack">The stack of parent <see cref="SelectItem"/>s.</param>
    private static void AddItem(SelectItem item, SortedSet<SelectItem> items, Stack<SelectItem> stack)
    {
        if (stack.TryPeek(out var parent))
        {
            parent.Items.Add(item);
        }
        else
        {
            items.Add(item);
        }
    }
}
