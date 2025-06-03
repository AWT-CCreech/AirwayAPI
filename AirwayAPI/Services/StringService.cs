using AirwayAPI.Services.Interfaces;
using System.Text.RegularExpressions;

namespace AirwayAPI.Services;

public partial class StringService : IStringService
{
    [GeneratedRegex("[;:,\\s]+")]
    private static partial Regex DelimiterRegex();

    public string ReplaceDelimiters(string input)
    {
        // If input is null or whitespace, just return it as-is
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        // Trim whitespace and replace all delimiters with a comma
        string replaced = DelimiterRegex().Replace(input.Trim(), ",");

        // Remove a leading comma if it exists
        if (replaced.StartsWith(','))
        {
            replaced = replaced[1..];
        }

        return replaced;
    }
}
