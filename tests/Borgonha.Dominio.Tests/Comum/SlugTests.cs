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

    [Theory]
    [InlineData("Brigadeiro de Maracujá", "brigadeiro-de-maracuja")]
    [InlineData("  Pão de Mel  ", "pao-de-mel")]
    [InlineData("M&M", "m-m")]
    [InlineData("Cookie Nº 1!", "cookie-n-1")]
    [InlineData("!!!", "produto")]
    public void GeraSlugAPartirDeUmTextoLivre(string texto, string esperado)
    {
        Assert.Equal(esperado, Slug.GerarAPartirDe(texto).Valor);
    }

    [Fact]
    public void LimitaOSlugGeradoAoTamanhoMaximo()
    {
        var slug = Slug.GerarAPartirDe(string.Join(' ', Enumerable.Repeat("chocolate", 20)));

        Assert.True(slug.Valor.Length <= Slug.TamanhoMaximo);
        Assert.False(slug.Valor.EndsWith('-'));
    }

    [Fact]
    public void AcrescentaSufixoNumerico()
    {
        Assert.Equal("brookie-2", Slug.Criar("brookie").ComSufixo(2).Valor);
        Assert.True(Slug.GerarAPartirDe(new string('a', Slug.TamanhoMaximo)).ComSufixo(12).Valor.Length <= Slug.TamanhoMaximo);
    }
}
