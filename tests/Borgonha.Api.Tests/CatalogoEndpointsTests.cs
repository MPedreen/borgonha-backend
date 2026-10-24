using System.Net;
using System.Net.Http.Json;
using Borgonha.Aplicacao.Catalogo;

namespace Borgonha.Api.Tests;

public sealed class CatalogoEndpointsTests(BorgonhaApiFactory fabrica) : IClassFixture<BorgonhaApiFactory>
{
    private static readonly string[] CategoriasEsperadas = ["cookies", "doces", "salgados"];

    private static readonly string[] CookiesEsperados =
        ["tradicional", "nutella", "bueno", "triplo-chocolate", "red-velvet", "mms"];

    private static readonly string[] ForaDoCardapioDeHoje = ["bolo-de-pote", "empadao-frango"];

    private readonly HttpClient _cliente = fabrica.CreateClient();

    [Fact]
    public async Task RetornaAsCategoriasNaOrdemDeExibicao()
    {
        var catalogo = await ObterCatalogoAsync();

        Assert.Equal(CategoriasEsperadas, catalogo.Categorias.Select(categoria => categoria.Slug));
    }

    [Fact]
    public async Task ListaOsSeisSaboresDeCookieNaOrdemDoCardapio()
    {
        var catalogo = await ObterCatalogoAsync();

        var cookies = catalogo.Categorias.Single(categoria => categoria.Slug == "cookies");
        Assert.Equal(CookiesEsperados, cookies.Produtos.Select(produto => produto.Slug));
        Assert.All(cookies.Produtos, produto => Assert.NotNull(produto.Frase));
        Assert.All(cookies.Produtos, produto => Assert.NotNull(produto.Peso));
        Assert.All(cookies.Produtos, produto => Assert.NotNull(produto.Preco));
    }

    [Fact]
    public async Task MostraOsPrecosDoCardapio()
    {
        var catalogo = await ObterCatalogoAsync();
        var precos = catalogo.Categorias
            .SelectMany(categoria => categoria.Produtos)
            .ToDictionary(produto => produto.Slug, produto => produto.Preco);

        Assert.Equal(10m, precos["tradicional"]);
        Assert.Equal(12m, precos["mms"]);
        Assert.Equal(15m, precos["brookie"]);
        Assert.Equal(19m, precos["matilda"]);
        Assert.Equal(10m, precos["bolinho-de-aipim"]);
    }

    [Fact]
    public async Task ProdutosForaDoCardapioDeHojeNaoAparecem()
    {
        var catalogo = await ObterCatalogoAsync();
        var slugs = catalogo.Categorias.SelectMany(categoria => categoria.Produtos).Select(produto => produto.Slug);

        Assert.All(ForaDoCardapioDeHoje, slug => Assert.DoesNotContain(slug, slugs));
        foreach (var slug in ForaDoCardapioDeHoje)
        {
            var resposta = await _cliente.GetAsync(new Uri($"/api/catalogo/{slug}", UriKind.Relative), TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        }
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
        var produto = await _cliente.GetFromJsonAsync<ProdutoDto>("/api/catalogo/brookie", TestContext.Current.CancellationToken);

        Assert.NotNull(produto);
        Assert.Equal("doces", produto.Categoria);
        Assert.Equal("170g", produto.Peso);
        Assert.Equal(15m, produto.Preco);
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
