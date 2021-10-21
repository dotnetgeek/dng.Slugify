using System.Globalization;
using System.Linq;

using dng.Slugify.Extensions;

using static System.String;

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
