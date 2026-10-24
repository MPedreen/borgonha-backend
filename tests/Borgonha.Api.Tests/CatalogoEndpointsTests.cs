using System.Net;
using System.Net.Http.Json;
using Borgonha.Aplicacao.Catalogo;

namespace Borgonha.Api.Tests;

public sealed class CatalogoEndpointsTests(BorgonhaApiFactory fabrica) : IClassFixture<BorgonhaApiFactory>
{
    private static readonly string[] CategoriasEsperadas = ["cookies", "doces", "salgados"];

    private static readonly string[] CookiesEsperados =
        ["tradicional", "nutella", "bueno", "triplo-chocolate", "red-velvet", "pistache", "bala-baiana", "mms"];

    private static readonly string[] SaboresDoBoloDePote =
        ["Chocolate & Maracujá", "Frutas Vermelhas & Ninho", "Nutella & Ninho"];

    private readonly HttpClient _cliente = fabrica.CreateClient();

    [Fact]
    public async Task RetornaAsCategoriasNaOrdemDeExibicao()
    {
        var catalogo = await ObterCatalogoAsync();

        Assert.Equal(CategoriasEsperadas, catalogo.Categorias.Select(categoria => categoria.Slug));
    }

    [Fact]
    public async Task ListaOsOitoSaboresDeCookieNaOrdemDoCardapio()
    {
        var catalogo = await ObterCatalogoAsync();

        var cookies = catalogo.Categorias.Single(categoria => categoria.Slug == "cookies");
        Assert.Equal(CookiesEsperados, cookies.Produtos.Select(produto => produto.Slug));
        Assert.All(cookies.Produtos, produto => Assert.NotNull(produto.Frase));
    }

    [Fact]
    public async Task SalgadosAindaNaoTemFoto()
    {
        var catalogo = await ObterCatalogoAsync();

        var salgados = catalogo.Categorias.Single(categoria => categoria.Slug == "salgados");
        Assert.All(salgados.Produtos, produto => Assert.Null(produto.Imagens.Inteiro));
    }

    [Fact]
    public async Task ObtemUmProdutoPeloSlug()
    {
        var produto = await _cliente.GetFromJsonAsync<ProdutoDto>("/api/catalogo/bolo-de-pote", TestContext.Current.CancellationToken);

        Assert.NotNull(produto);
        Assert.Equal("doces", produto.Categoria);
        Assert.Equal(SaboresDoBoloDePote, produto.Variacoes);
    }

    [Theory]
    [InlineData("nao-existe")]
    [InlineData("Slug Invalido")]
    public async Task RespondeNotFoundEmProblemDetailsParaProdutoInexistente(string slug)
    {
        var resposta = await _cliente.GetAsync(
            new Uri($"/api/catalogo/{Uri.EscapeDataString(slug)}", UriKind.Relative),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Equal("application/problem+json", resposta.Content.Headers.ContentType?.MediaType);
    }

    private async Task<CatalogoDto> ObterCatalogoAsync()
    {
        var catalogo = await _cliente.GetFromJsonAsync<CatalogoDto>("/api/catalogo", TestContext.Current.CancellationToken);

        Assert.NotNull(catalogo);
        return catalogo;
    }
}
