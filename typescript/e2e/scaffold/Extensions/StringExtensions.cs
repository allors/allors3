namespace Scaffold
{
    using System.Globalization;

    public static class StringExtensions
    {
        public static string ToPascalCase(this string @this) =>
            new(CultureInfo.InvariantCulture.TextInfo
                .ToTitleCase(@this)
                .Where(char.IsLetterOrDigit)
                .ToArray());
    }
}
