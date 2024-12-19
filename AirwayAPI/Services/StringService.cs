using AirwayAPI.Services.Interfaces;
using System.Text.RegularExpressions;

namespace AirwayAPI.Services
{
    public partial class StringService : IStringService
    {
        [GeneratedRegex("[;:,\\s]+")]
        private static partial Regex DelimiterRegex();

        public string ReplaceDelimiters(string input)
        {
            return string.IsNullOrWhiteSpace(input)
                ? input
                : DelimiterRegex().Replace(input, ",");
        }
    }
}
