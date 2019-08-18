using Xunit;

namespace dng.Slugify.Tests
{
    public class UnidecoderTest
    {
        [Trait("Project", "dng.Slugify.Unidecoder")]
        [Theory(DisplayName = "Should unidecode characters correctly")]
        [InlineData("Não Pão da Avó", "Nao Pao da Avo")]
        [InlineData("férias", "ferias")]
        [InlineData("э ю я", "e yu ya")]
        [InlineData("ф х ц ч ш щ", "f kh ts ch sh shch")]
        [InlineData("ä ö ü ß Ä Ö Ü ẞ" , "ae oe ue ss Ae Oe Ue Ss")]
        [InlineData("ß ẞ § $ % & ! \" # '", "ss Ss § $ % & ! \" # '")]
        public void ShouldUnidecodeCharacterCorrectly(
            string input,
            string expectation)
        {
            var decoded = Unidecoder.Decode(input);

            Assert.Equal(expectation, decoded);
        }
    }
}
