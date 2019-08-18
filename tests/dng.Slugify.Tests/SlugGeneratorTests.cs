using Xunit;

namespace dng.Slugify.Tests
{
    public class SlugGeneratorTests
    {
        [Trait("Project", "dng.Slugify.SlugGenerator")]
        [Theory(DisplayName = "Should generate valid url")]
        [InlineData("Der Vater und der Sohn", "der-vater-und-der-sohn")]
        [InlineData("Die Väter mit den Söhnen", "die-vaeter-mit-den-soehnen")]
        [InlineData("Macht das Spiel Spaß?", "macht-das-spiel-spass")]
        [InlineData("Das Spiel macht Spaß!", "das-spiel-macht-spass")]
        [InlineData("§ Das ist % \"eine\" Url § mit $ 'Sonderzeichen' !", "das-ist-eine-url-mit-sonderzeichen")]
        public void ShouldGenerateValidUrl(
            string input,
            string expected)
        {
            var slugGenerator = new SlugGenerator();
            var decoded = slugGenerator.Generate(input);

            Assert.Equal(expected, decoded);
        }

        [Trait("Project", "dng.Slugify.SlugGenerator")]
        [Theory(DisplayName = "Should replace all whitespaces correctly")]
        [InlineData("Der      Vater und  der   Sohn", "der-vater-und-der-sohn")]
        [InlineData("Der Vater und  der   Sohn  ", "der-vater-und-der-sohn")]
        [InlineData("  Der Vater und  der   Sohn", "der-vater-und-der-sohn")]
        [InlineData("  Der Vater und  der   Sohn  ", "der-vater-und-der-sohn")]
        public void ShouldReplaceAllWhitespacesCorrectly(
            string input,
            string expected)
        {
            var slugGenerator = new SlugGenerator();
            var decoded = slugGenerator.Generate(input);

            Assert.Equal(expected, decoded);
        }
    }
}
