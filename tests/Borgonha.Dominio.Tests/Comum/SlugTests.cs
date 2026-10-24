using Borgonha.Dominio.Comum;

namespace Borgonha.Dominio.Tests.Comum;

public sealed class SlugTests
{
    [Theory]
    [InlineData("mms")]
    [InlineData("red-velvet")]
    [InlineData("bolo-de-pote")]
    [InlineData("cookie-2")]
    public void AceitaLetrasMinusculasNumerosEHifens(string valor)
    {
        var slug = Slug.Criar(valor);

        Assert.Equal(valor, slug.Valor);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Red-Velvet")]
    [InlineData("red velvet")]
    [InlineData("-red")]
    [InlineData("red-")]
    [InlineData("red--velvet")]
    [InlineData("pão")]
    public void RecusaFormatosInvalidos(string valor)
    {
        Assert.False(Slug.TentarCriar(valor, out _));
        Assert.Throws<DominioException>(() => Slug.Criar(valor));
    }

    [Fact]
    public void RecusaTextoMaiorQueOLimite()
    {
        var valor = new string('a', Slug.TamanhoMaximo + 1);

        Assert.False(Slug.TentarCriar(valor, out _));
    }

    [Fact]
    public void SlugsComOMesmoValorSaoIguais()
    {
        Assert.Equal(Slug.Criar("nutella"), Slug.Criar("nutella"));
    }
}
