using System.Net;
using System.Text;
using Borgonha.Aplicacao.Entrega;
using Borgonha.Dominio.Lojas;
using Borgonha.Infraestrutura.Mapas;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Borgonha.Api.Tests;

/// <summary>
/// O cliente do Geoapify contra respostas no formato do serviço de verdade: nenhum teste gasta crédito.
/// </summary>
public sealed class ClienteGeoapifyTests
{
    private const string RespostaDeEnderecos = """
        {"type":"FeatureCollection","features":[
          {"type":"Feature","properties":{"lon":-42.980427,"lat":-22.802102,"formatted":"Rua Afranio Peixoto 25, Santa Luzia, São Gonçalo - Sudeste, 24725-270, Brasil",
           "address_line1":"Rua Afrânio Peixoto 25","address_line2":"Santa Luzia, São Gonçalo - RJ, 24725-270, Brasil","result_type":"building"}},
          {"type":"Feature","properties":{"lon":-42.980427,"lat":-22.802102,"address_line1":"Rua Afrânio Peixoto 25","address_line2":"Santa Luzia, São Gonçalo - RJ, 24725-270, Brasil"}},
          {"type":"Feature","properties":{"lon":-43.1,"lat":-22.5,"address_line1":"Rua Afrânio Peixoto 25","address_line2":"Quarteirão Ingelheim, Petrópolis - RJ, 25660-002, Brasil"}},
          {"type":"Feature","properties":{"formatted":"Sem coordenadas"}}
        ]}
        """;

    private const string RespostaDeTrajeto = """
        {"type":"FeatureCollection","features":[{"type":"Feature","properties":{"mode":"drive","distance":1271,"distance_units":"meters","time":135.758}}]}
        """;

    private static readonly Coordenadas Loja = new(-22.8021494, -42.9803336);
    private static readonly Coordenadas Cliente = new(-22.8105, -42.9805);

    private static CancellationToken Cancelamento => TestContext.Current.CancellationToken;

    [Fact]
    public async Task LeAsSugestoesEDescartaRepetidasEIncompletas()
    {
        var geoapify = new Montagem(RespostaDeEnderecos);

        var sugestoes = await geoapify.Cliente.BuscarAsync("Rua Afrânio Peixoto 25", Loja, Cancelamento);

        Assert.Equal(2, sugestoes.Count);
        Assert.Equal("Rua Afrânio Peixoto 25", sugestoes[0].Titulo);
        Assert.Equal("Santa Luzia, São Gonçalo - RJ, 24725-270", sugestoes[0].Detalhe);
        Assert.Equal(-22.802102, sugestoes[0].Latitude);
        Assert.Equal(-42.980427, sugestoes[0].Longitude);
        Assert.Contains("Petrópolis", sugestoes[1].Detalhe, StringComparison.Ordinal);
    }

    [Fact]
    public async Task EnviaAChaveNoCabecalhoEPedeSoEnderecosDoBrasilPertoDaLoja()
    {
        var geoapify = new Montagem(RespostaDeEnderecos);

        await geoapify.Cliente.BuscarAsync("Rua Afrânio, 40", Loja, Cancelamento);

        var requisicao = Assert.Single(geoapify.Requisicoes);
        Assert.Equal("chave-de-teste", requisicao.Chave);
        Assert.DoesNotContain("chave-de-teste", requisicao.Url, StringComparison.Ordinal);
        Assert.Contains("filter=countrycode:br", requisicao.Url, StringComparison.Ordinal);
        Assert.Contains("bias=proximity:-42.9803336,-22.8021494", requisicao.Url, StringComparison.Ordinal);
        Assert.Contains("text=Rua%20Afr%C3%A2nio%2C%2040", requisicao.Url, StringComparison.Ordinal);
    }

    [Fact]
    public async Task OMesmoEnderecoDigitadoDeOutroJeitoNaoGastaCreditoDeNovo()
    {
        var geoapify = new Montagem(RespostaDeEnderecos);

        await geoapify.Cliente.BuscarAsync("Rua Afrânio Peixoto, 25", Loja, Cancelamento);
        await geoapify.Cliente.BuscarAsync("  rua afranio   peixoto 25 ", Loja, Cancelamento);

        Assert.Single(geoapify.Requisicoes);
        Assert.Equal(1, geoapify.Creditos.Gastos);
    }

    [Fact]
    public async Task CalculaOTrajetoEGuardaParaOsProximosClientesDaMesmaRegiao()
    {
        var geoapify = new Montagem(RespostaDeTrajeto);

        var metros = await geoapify.Cliente.DistanciaEmMetrosAsync(Loja, Cliente, Cancelamento);
        var denovo = await geoapify.Cliente.DistanciaEmMetrosAsync(Loja, new Coordenadas(-22.810501, -42.980501), Cancelamento);

        Assert.Equal(1_271, metros);
        Assert.Equal(1_271, denovo);
        var requisicao = Assert.Single(geoapify.Requisicoes);
        Assert.Contains("waypoints=-22.8021494,-42.9803336%7C-22.8105,-42.9805", requisicao.Url, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PontoSemRuaPertoViraRotaNaoEncontrada()
    {
        var geoapify = new Montagem("""{"statusCode":400,"error":"Bad Request","message":"No suitable edges near location."}""", HttpStatusCode.BadRequest);

        await Assert.ThrowsAsync<RotaNaoEncontradaException>(() => geoapify.Cliente.DistanciaEmMetrosAsync(Loja, Cliente, Cancelamento));
    }

    [Fact]
    public async Task SemChaveNemChegaAChamarOServico()
    {
        var geoapify = new Montagem(RespostaDeEnderecos, chave: null);

        await Assert.ThrowsAsync<MapasIndisponiveisException>(() => geoapify.Cliente.BuscarAsync("Rua Afrânio", Loja, Cancelamento));
        Assert.Empty(geoapify.Requisicoes);
    }

    [Fact]
    public async Task DepoisDoLimiteDoDiaNaoChamaMaisOServicoMasAindaRespondeDoCache()
    {
        var geoapify = new Montagem(RespostaDeEnderecos, creditosPorDia: 1);

        await geoapify.Cliente.BuscarAsync("Rua Afrânio 25", Loja, Cancelamento);

        await Assert.ThrowsAsync<MapasIndisponiveisException>(() => geoapify.Cliente.BuscarAsync("Rua Outra 10", Loja, Cancelamento));
        var doCache = await geoapify.Cliente.BuscarAsync("rua afranio 25", Loja, Cancelamento);
        Assert.Equal(2, doCache.Count);
        Assert.Single(geoapify.Requisicoes);
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    public async Task RecusaOuErroDoServicoViraIndisponivel(HttpStatusCode status)
    {
        var geoapify = new Montagem("{}", status);

        await Assert.ThrowsAsync<MapasIndisponiveisException>(() => geoapify.Cliente.BuscarAsync("Rua Afrânio", Loja, Cancelamento));
    }

    [Fact]
    public async Task AposRecusaPorExcessoDeUsoPausaAsChamadas()
    {
        var geoapify = new Montagem("{}", HttpStatusCode.TooManyRequests);

        await Assert.ThrowsAsync<MapasIndisponiveisException>(() => geoapify.Cliente.BuscarAsync("Rua Afrânio", Loja, Cancelamento));
        await Assert.ThrowsAsync<MapasIndisponiveisException>(() => geoapify.Cliente.BuscarAsync("Outra rua", Loja, Cancelamento));

        Assert.Single(geoapify.Requisicoes);
    }

    [Fact]
    public async Task RespostaQueNaoEJsonViraIndisponivel()
    {
        var geoapify = new Montagem("<html>erro</html>");

        await Assert.ThrowsAsync<MapasIndisponiveisException>(() => geoapify.Cliente.BuscarAsync("Rua Afrânio", Loja, Cancelamento));
    }

    [Fact]
    public async Task ServicoQueNaoRespondeViraIndisponivel()
    {
        var geoapify = new Montagem(erro: new HttpRequestException("sem rede"));

        await Assert.ThrowsAsync<MapasIndisponiveisException>(() => geoapify.Cliente.BuscarAsync("Rua Afrânio", Loja, Cancelamento));
    }

    private sealed class Montagem
    {
        public Montagem(
            string? corpo = null,
            HttpStatusCode status = HttpStatusCode.OK,
            string? chave = "chave-de-teste",
            int creditosPorDia = 100,
            Exception? erro = null)
        {
            var opcoes = Options.Create(new OpcoesDaGeoapify { Chave = chave, CreditosPorDia = creditosPorDia });
            Creditos = new ControleDeCreditos(opcoes, TimeProvider.System);

            var http = new HttpClient(new Resposta(Requisicoes, corpo, status, erro)) { BaseAddress = opcoes.Value.UrlBase };
            Cliente = new ClienteGeoapify(http, opcoes, Creditos, new CacheDeMapas(), NullLogger<ClienteGeoapify>.Instance);
        }

        public List<RequisicaoFeita> Requisicoes { get; } = [];

        public ControleDeCreditos Creditos { get; }

        public ClienteGeoapify Cliente { get; }
    }

    private sealed record RequisicaoFeita(string Url, string? Chave);

    private sealed class Resposta(List<RequisicaoFeita> requisicoes, string? corpo, HttpStatusCode status, Exception? erro) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            requisicoes.Add(new RequisicaoFeita(
                request.RequestUri!.PathAndQuery,
                request.Headers.TryGetValues("X-API-Key", out var valores) ? valores.Single() : null));

            if (erro is not null)
            {
                throw erro;
            }

            return Task.FromResult(new HttpResponseMessage(status)
            {
                Content = new StringContent(corpo ?? string.Empty, Encoding.UTF8, "application/json"),
            });
        }
    }
}
