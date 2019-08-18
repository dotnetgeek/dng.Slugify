using System.Globalization;
using System.Linq;
using static System.String;
using dng.Slugify.Extensions;

namespace dng.Slugify
{
    public class SlugGenerator
    {
        public string Generate(
            string input)
        {
            if (IsNullOrWhiteSpace(input))
                return Empty;

            return input
                .ToLowerInvariant()
                .Decode()
                .RemoveInvalidCharacters()
                .CleanWhiteSpace();
        }
    }
}
