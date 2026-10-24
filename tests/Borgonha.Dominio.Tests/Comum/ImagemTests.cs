using Borgonha.Dominio.Comum;

namespace Borgonha.Dominio.Tests.Comum;

public sealed class ImagemTests
{
    [Fact]
    public void AceitaCaminhoRelativoSemExtensao()
    {
        var imagem = new Imagem("cookies/nutella-partido", "Cookie de Nutella partido ao meio");

        Assert.Equal("cookies/nutella-partido", imagem.Caminho);
        Assert.Equal("Cookie de Nutella partido ao meio", imagem.TextoAlternativo);
    }

    [Theory]
    [InlineData("/cookies/nutella")]
    [InlineData("cookies/nutella.webp")]
    [InlineData("https://exemplo.com/nutella")]
    [InlineData("cookies//nutella")]
    public void RecusaCaminhoInvalido(string caminho)
    {
        Assert.Throws<DominioException>(() => new Imagem(caminho, "Texto alternativo"));
    }

    [Fact]
    public void ExigeTextoAlternativo()
    {
        Assert.Throws<DominioException>(() => new Imagem("cookies/nutella", "  "));
    }
}
