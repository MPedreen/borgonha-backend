using System.Net;
using System.Net.Http.Json;
using Borgonha.Aplicacao.Administracao;
using Borgonha.Aplicacao.Catalogo;
using Microsoft.AspNetCore.Mvc;
using SkiaSharp;

namespace Borgonha.Api.Tests;

public sealed class AreaDoAdministradorTests(BorgonhaApiFactory fabrica) : IClassFixture<BorgonhaApiFactory>
{
    private const string Cabecalho = "X-Borgonha-Admin";

    private static readonly string[] SituacoesValidas = ["no-cardapio", "esgotado", "oculto"];

    private static CancellationToken Cancelamento => TestContext.Current.CancellationToken;

    [Fact]
    public async Task RecusaSenhaErrada()
    {
        using var cliente = CriarCliente();

        var resposta = await EntrarAsync(cliente, BorgonhaApiFactory.UsuarioAdmin, "senha-errada");

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
        var problema = await resposta.Content.ReadFromJsonAsync<ProblemDetails>(Cancelamento);
        Assert.Equal("Usuário ou senha incorretos.", problema?.Title);
        Assert.False(resposta.Headers.Contains("Set-Cookie"));
    }

    [Fact]
    public async Task RecusaUsuarioErrado()
    {
        using var cliente = CriarCliente();

        var resposta = await EntrarAsync(cliente, "outra-pessoa", BorgonhaApiFactory.SenhaAdmin);

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task AceitaOUsuarioComLetrasMaiusculas()
    {
        using var cliente = CriarCliente();

        var resposta = await EntrarAsync(cliente, " Dona-Da-Loja ", BorgonhaApiFactory.SenhaAdmin);

        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
    }

    [Fact]
    public async Task AbreASessaoNumCookieProtegido()
    {
        using var cliente = CriarCliente();

        var resposta = await EntrarAsync(cliente, BorgonhaApiFactory.UsuarioAdmin, BorgonhaApiFactory.SenhaAdmin);

        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
        var cookie = Assert.Single(resposta.Headers.GetValues("Set-Cookie"));
        Assert.StartsWith("__Host-borgonha-admin=", cookie, StringComparison.Ordinal);
        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("secure", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=strict", cookie, StringComparison.OrdinalIgnoreCase);

        var sessao = await cliente.GetAsync(new Uri("/api/admin/sessao", UriKind.Relative), Cancelamento);
        Assert.Equal(HttpStatusCode.NoContent, sessao.StatusCode);
    }

    [Fact]
    public async Task ExigeOCabecalhoDaAreaDoAdministrador()
    {
        using var cliente = fabrica.CriarClienteHttps();

        var resposta = await cliente.PostAsJsonAsync(
            new Uri("/api/admin/sessao", UriKind.Relative),
            new { usuario = BorgonhaApiFactory.UsuarioAdmin, senha = BorgonhaApiFactory.SenhaAdmin },
            Cancelamento);

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }

    [Theory]
    [InlineData("/api/admin/sessao")]
    [InlineData("/api/admin/cardapio")]
    public async Task ExigeSessaoAberta(string caminho)
    {
        using var cliente = CriarCliente();

        var resposta = await cliente.GetAsync(new Uri(caminho, UriKind.Relative), Cancelamento);

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task SairEncerraASessao()
    {
        using var cliente = await CriarClienteComSessaoAsync();

        var saida = await cliente.DeleteAsync(new Uri("/api/admin/sessao", UriKind.Relative), Cancelamento);
        var sessao = await cliente.GetAsync(new Uri("/api/admin/sessao", UriKind.Relative), Cancelamento);

        Assert.Equal(HttpStatusCode.NoContent, saida.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, sessao.StatusCode);
    }

    [Fact]
    public async Task ListaTodosOsProdutosComASituacao()
    {
        using var cliente = await CriarClienteComSessaoAsync();

        var cardapio = await cliente.GetFromJsonAsync<CardapioAdminDto>(new Uri("/api/admin/cardapio", UriKind.Relative), Cancelamento);

        var cookies = Assert.Single(cardapio!.Categorias, categoria => categoria.Slug == "cookies");
        Assert.Contains(cookies.Produtos, produto => produto.Slug == "nutella" && produto.Peso == "130g");
        Assert.All(cookies.Produtos, produto => Assert.Contains(produto.Situacao, SituacoesValidas));
    }

    [Fact]
    public async Task CriaEditaEExcluiUmProduto()
    {
        using var cliente = await CriarClienteComSessaoAsync();
        using var visitante = fabrica.CriarClienteHttps();

        var produto = await CriarProdutoAsync(cliente, "Brigadeiro de Pistache");
        Assert.Equal("brigadeiro-de-pistache", produto.Slug);
        Assert.Equal("doces", produto.Categoria);
        Assert.Equal("no-cardapio", produto.Situacao);

        var edicao = await cliente.PutAsJsonAsync(
            new Uri($"/api/admin/produtos/{produto.Slug}", UriKind.Relative),
            new DadosDoProdutoDto("Brigadeiro de Pistache", "Feito com pistache de verdade.", null, "25g", 8.5m, null),
            Cancelamento);
        var editado = await edicao.Content.ReadFromJsonAsync<ProdutoAdminDto>(Cancelamento);
        Assert.Equal("Feito com pistache de verdade.", editado?.Descricao);
        Assert.Equal("25g", editado?.Peso);
        Assert.Equal(8.5m, editado?.Preco);

        var noSite = await ObterProdutoNoSiteAsync(visitante, produto.Slug);
        Assert.Equal(HttpStatusCode.OK, noSite.StatusCode);

        var exclusao = await cliente.DeleteAsync(new Uri($"/api/admin/produtos/{produto.Slug}", UriKind.Relative), Cancelamento);
        Assert.Equal(HttpStatusCode.NoContent, exclusao.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await ObterProdutoNoSiteAsync(visitante, produto.Slug)).StatusCode);
    }

    [Fact]
    public async Task CadastraProdutoSoComNomeEPreco()
    {
        using var cliente = await CriarClienteComSessaoAsync();
        using var visitante = fabrica.CriarClienteHttps();

        var resposta = await cliente.PostAsJsonAsync(
            new Uri("/api/admin/produtos", UriKind.Relative),
            new NovoProdutoDto("doces", "Pavê de teste", null, null, null, 19m, null),
            Cancelamento);

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var produto = await resposta.Content.ReadFromJsonAsync<ProdutoAdminDto>(Cancelamento);
        Assert.Null(produto?.Descricao);
        Assert.Equal(19m, produto?.Preco);

        var noSite = await visitante.GetFromJsonAsync<ProdutoDto>(new Uri("/api/catalogo/pave-de-teste", UriKind.Relative), Cancelamento);
        Assert.Equal(19m, noSite?.Preco);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(12.999)]
    [InlineData(10000)]
    public async Task RecusaPrecoInvalido(double preco)
    {
        using var cliente = await CriarClienteComSessaoAsync();
        var produto = await CriarProdutoAsync(cliente, $"Preço inválido {preco}");

        var resposta = await cliente.PutAsJsonAsync(
            new Uri($"/api/admin/produtos/{produto.Slug}", UriKind.Relative),
            new DadosDoProdutoDto(produto.Nome, null, null, null, (decimal)preco, null),
            Cancelamento);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("preço", (await resposta.Content.ReadFromJsonAsync<ProblemDetails>(Cancelamento))?.Title, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProdutoComNomeRepetidoGanhaOutroSlug()
    {
        using var cliente = await CriarClienteComSessaoAsync();

        var primeiro = await CriarProdutoAsync(cliente, "Pudim");
        var segundo = await CriarProdutoAsync(cliente, "Pudim");

        Assert.Equal("pudim", primeiro.Slug);
        Assert.Equal("pudim-2", segundo.Slug);
    }

    [Fact]
    public async Task OcultarTiraOProdutoDoSiteNaHora()
    {
        using var cliente = await CriarClienteComSessaoAsync();
        // Um visitante sem sessão, como o site: as respostas dele ficam no cache de 5 minutos.
        using var visitante = fabrica.CriarClienteHttps();
        var produto = await CriarProdutoAsync(cliente, "Torta de Limão");
        Assert.Contains(await ListarSlugsDoSiteAsync(visitante), slug => slug == produto.Slug);
        Assert.Equal(HttpStatusCode.OK, (await ObterProdutoNoSiteAsync(visitante, produto.Slug)).StatusCode);

        var resposta = await AlterarSituacaoAsync(cliente, produto.Slug, "oculto");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        // A alteração limpa o cache na hora: o visitante já não vê o produto.
        Assert.DoesNotContain(await ListarSlugsDoSiteAsync(visitante), slug => slug == produto.Slug);
        Assert.Equal(HttpStatusCode.NotFound, (await ObterProdutoNoSiteAsync(visitante, produto.Slug)).StatusCode);

        await AlterarSituacaoAsync(cliente, produto.Slug, "esgotado");
        var esgotado = await (await ObterProdutoNoSiteAsync(visitante, produto.Slug)).Content.ReadFromJsonAsync<ProdutoDto>(Cancelamento);
        Assert.False(esgotado?.Disponivel);
    }

    [Fact]
    public async Task RecusaDadosInvalidosComMensagemClara()
    {
        using var cliente = await CriarClienteComSessaoAsync();

        var semNome = await cliente.PostAsJsonAsync(
            new Uri("/api/admin/produtos", UriKind.Relative),
            new NovoProdutoDto("doces", " ", "Descrição.", null, null, null, null),
            Cancelamento);
        var situacaoInvalida = await AlterarSituacaoAsync(cliente, "nutella", "sumido");

        Assert.Equal(HttpStatusCode.BadRequest, semNome.StatusCode);
        Assert.Contains("Nome do produto", (await semNome.Content.ReadFromJsonAsync<ProblemDetails>(Cancelamento))?.Title, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.BadRequest, situacaoInvalida.StatusCode);
    }

    [Fact]
    public async Task TrocaAFotoPorVersoesOtimizadasEmWebp()
    {
        using var cliente = await CriarClienteComSessaoAsync();
        var produto = await CriarProdutoAsync(cliente, "Cookie de Teste");

        var resposta = await EnviarFotoAsync(cliente, produto.Slug, CriarPng(1200, 900), "foto.png");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var atualizado = await resposta.Content.ReadFromJsonAsync<ProdutoAdminDto>(Cancelamento);
        var foto = atualizado?.Imagens.Inteiro;
        Assert.NotNull(foto);
        Assert.Matches("^envios/[0-9a-f]{32}$", foto.Caminho);
        Assert.Equal("Cookie de Teste", foto.Alt);

        var arquivo = await cliente.GetAsync(new Uri($"/api/imagens/{foto.Caminho}-640.webp", UriKind.Relative), Cancelamento);
        Assert.Equal(HttpStatusCode.OK, arquivo.StatusCode);
        Assert.Equal("image/webp", arquivo.Content.Headers.ContentType?.MediaType);
        using var webp = SKBitmap.Decode(await arquivo.Content.ReadAsByteArrayAsync(Cancelamento));
        Assert.Equal(640, webp.Width);

        var remocao = await cliente.DeleteAsync(new Uri($"/api/admin/produtos/{produto.Slug}/fotos/inteiro", UriKind.Relative), Cancelamento);
        Assert.Equal(HttpStatusCode.OK, remocao.StatusCode);
        Assert.Empty(Directory.EnumerateFiles(Path.Combine(fabrica.PastaDasImagens, "envios"), $"{foto.Caminho[7..]}-*"));
    }

    [Fact]
    public async Task RecusaArquivoQueNaoEImagem()
    {
        using var cliente = await CriarClienteComSessaoAsync();

        var resposta = await EnviarFotoAsync(cliente, "nutella", "<script>alert(1)</script>"u8.ToArray(), "foto.jpg");

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("não é uma foto aceita", (await resposta.Content.ReadFromJsonAsync<ProblemDetails>(Cancelamento))?.Title, StringComparison.Ordinal);
    }

    private HttpClient CriarCliente()
    {
        var cliente = fabrica.CriarClienteHttps();
        cliente.DefaultRequestHeaders.Add(Cabecalho, "1");
        return cliente;
    }

    private async Task<HttpClient> CriarClienteComSessaoAsync()
    {
        var cliente = CriarCliente();
        var resposta = await EntrarAsync(cliente, BorgonhaApiFactory.UsuarioAdmin, BorgonhaApiFactory.SenhaAdmin);
        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
        return cliente;
    }

    private static Task<HttpResponseMessage> EntrarAsync(HttpClient cliente, string usuario, string senha) =>
        cliente.PostAsJsonAsync(new Uri("/api/admin/sessao", UriKind.Relative), new { usuario, senha }, Cancelamento);

    private static async Task<ProdutoAdminDto> CriarProdutoAsync(HttpClient cliente, string nome)
    {
        var resposta = await cliente.PostAsJsonAsync(
            new Uri("/api/admin/produtos", UriKind.Relative),
            new NovoProdutoDto("doces", nome, "Descrição do produto.", null, "100g", 12m, null),
            Cancelamento);

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        return (await resposta.Content.ReadFromJsonAsync<ProdutoAdminDto>(Cancelamento))!;
    }

    private static Task<HttpResponseMessage> AlterarSituacaoAsync(HttpClient cliente, string slug, string situacao) =>
        cliente.PutAsJsonAsync(new Uri($"/api/admin/produtos/{slug}/situacao", UriKind.Relative), new SituacaoDoProdutoDto(situacao), Cancelamento);

    private static Task<HttpResponseMessage> ObterProdutoNoSiteAsync(HttpClient cliente, string slug) =>
        cliente.GetAsync(new Uri($"/api/catalogo/{slug}", UriKind.Relative), Cancelamento);

    private static async Task<IEnumerable<string>> ListarSlugsDoSiteAsync(HttpClient cliente)
    {
        var catalogo = await cliente.GetFromJsonAsync<CatalogoDto>(new Uri("/api/catalogo", UriKind.Relative), Cancelamento);
        return catalogo!.Categorias.SelectMany(categoria => categoria.Produtos).Select(produto => produto.Slug);
    }

    private static async Task<HttpResponseMessage> EnviarFotoAsync(HttpClient cliente, string slug, byte[] conteudo, string nomeDoArquivo)
    {
        using var formulario = new MultipartFormDataContent();
        using var arquivo = new ByteArrayContent(conteudo);
        formulario.Add(arquivo, "foto", nomeDoArquivo);

        return await cliente.PutAsync(new Uri($"/api/admin/produtos/{slug}/fotos/inteiro", UriKind.Relative), formulario, Cancelamento);
    }

    private static byte[] CriarPng(int largura, int altura)
    {
        using var bitmap = new SKBitmap(largura, altura);
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(new SKColor(133, 34, 36));
        }

        using var imagem = SKImage.FromBitmap(bitmap);
        using var dados = imagem.Encode(SKEncodedImageFormat.Png, 100);
        return dados.ToArray();
    }
}
