using EnumerableExtensions.Exceptions;
using EnumerableExtensions.Parsing;
using System.Diagnostics.CodeAnalysis;

namespace EnumerableExtensions.Tests;

[ExcludeFromCodeCoverage]
public class SelectParserTests
{
    [Theory]
    [InlineData("", 0)] // Empty input
    [InlineData(null, 0)] // Null input
    [InlineData(",", 0)] // Starts with a comma
    [InlineData("()", 0)] // Empty parentheses
    [InlineData(")a", 0)] // Closing parenthesis without opening one
    [InlineData("a()", 2)] // Field followed by empty parentheses
    [InlineData("a(b)c", 4)] // Invalid character 'c' after parenthesis
    [InlineData("a,,c", 2)] // Double commas
    [InlineData("a(b(,", 4)] // Unbalanced parentheses and comma
    [InlineData("a,(", 2)] // Comma followed by an open parenthesis
    [InlineData("a((", 2)] // Double open parentheses
    [InlineData("a(b)(", 4)] // Open parenthesis after closing one
    [InlineData("a(b))", 4)] // Extra closing parenthesis
    [InlineData("a(1b)", 2)] // Invalid character '1' in field name
    [InlineData("a(b,c", 4)] // Missing closing parenthesis
    [InlineData("a(,)", 2)] // Empty value between parentheses
    [InlineData("a(b, (c))", 7)] // Incorrect comma usage after opening parenthesis
    public void ParseSelect_IncorrectSelectExpression_ShouldThrowException(string select, int position)
    {
        Assert.ThrowsAny<InvalidSelectExpressionException>(() => SelectRecursiveParser.ParseSelect(select));
    }

    [Theory]
    [InlineData("a", "a")]
    [InlineData("a,b", "a,b")]
    [InlineData("b,a", "a,b")]
    [InlineData("b,a,c(b,a)", "a,b,c(a,b)")]
    [InlineData("a(b(c),a)", "a(a,b(c))")]
    public void ParseSelect_SelectExpression_ShouldReturnSelectItems(string select, string expected)
    {
        var selectItems = SelectParser.ParseSelect(select);
        var result = Print(selectItems);

        Assert.NotEmpty(selectItems);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("a", "a")]
    [InlineData("a,b", "a,b")]
    [InlineData("b,a", "a,b")]
    [InlineData("a(b,c)", "a(b,c)")]
    [InlineData("b(a,c)", "b(a,c)")]
    [InlineData("a(b(c),d)", "a(b(c),d)")]
    [InlineData("a(b,c(d))", "a(b,c(d))")]
    [InlineData("a(b(c),d(e,f))", "a(b(c),d(e,f))")]
    [InlineData("a(b(c,d,e),f)", "a(b(c,d,e),f)")]
    [InlineData("a(b(c(d,e),f))", "a(b(c(d,e),f))")]
    public void ParseSelect3_SelectExpression_ShouldReturnSelectItems(string select, string expected)
    {
        var selectItems = SelectRecursiveParser.ParseSelect(select);
        var result = Print(selectItems);

        Assert.NotEmpty(selectItems);
        Assert.Equal(expected, result);
    }

    private static string Print(SortedSet<SelectItem> items)
    {
        return string.Join(',', items.Select(i => string.Format("{0}{1}", i.Name, i.Items.Count != 0 ? $"({Print(i.Items)})" : string.Empty)));
    }
}
