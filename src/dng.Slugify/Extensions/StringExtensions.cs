using System.Text.RegularExpressions;

namespace dng.Slugify.Extensions
{
    internal static class StringExtensions
    {
        internal static Regex CleanWhiteSpaceRegex = new Regex(@"[\s\-]+", RegexOptions.Compiled);
        internal static Regex RemoveInvalidCharactersRegex = new Regex(@"[^a-zA-Z0-9\-_\s]", RegexOptions.Compiled);

        /// <summary>
        /// convert multiple spaces into one space   
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        internal static string CleanWhiteSpace(
            this string input,
            string separator = "-")
        {
            return CleanWhiteSpaceRegex.Replace(input.Trim(), separator);
        }

        /// <summary>
        /// Remove all invalid characters
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        internal static string RemoveInvalidCharacters(
            this string input)
        {
            return RemoveInvalidCharactersRegex.Replace(input, string.Empty);
        }

        internal static string Decode(
            this string input)
        {
            return Unidecoder.Decode(input);
        }
    }
}
