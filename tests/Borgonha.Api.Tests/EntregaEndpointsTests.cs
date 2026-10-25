using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using Borgonha.Aplicacao.Entrega;
using Microsoft.AspNetCore.Mvc;

namespace Borgonha.Api.Tests;

public sealed class EntregaEndpointsTests(BorgonhaApiFactory fabrica) : IClassFixture<BorgonhaApiFactory>
{
    private const double LatitudeDaLoja = -22.8021494;
    private const double LongitudeDaLoja = -42.9803336;
    private const double MetrosPorGrauDeLatitude = 111_195;

    private static CancellationToken Cancelamento => TestContext.Current.CancellationToken;

    private readonly HttpClient _cliente = fabrica.CreateClient();

    [Fact]
    public async Task SugereEnderecosComCoordenadas()
    {
        fabrica.Mapas.Reiniciar();

        var sugestoes = await _cliente.GetFromJsonAsync<List<EnderecoSugeridoDto>>(
            "/api/entrega/enderecos?texto=Rua%20Afranio%20Peixoto%2025",
            Cancelamento);

        var sugestao = Assert.Single(sugestoes!);
        Assert.Equal("Rua Afranio Peixoto 25", sugestao.Titulo);
        Assert.Equal("Santa Luzia, São Gonçalo - RJ, 24725-270", sugestao.Detalhe);
        Assert.Equal(-22.802102, sugestao.Latitude);
    }

    [Theory]
    [InlineData("")]
    [InlineData("rua")]
    [InlineData("   ab   ")]
    public async Task RecusaTextoCurtoSemChamarOMapa(string texto)
    {
        fabrica.Mapas.Reiniciar();

        var resposta = await _cliente.GetAsync($"/api/entrega/enderecos?texto={Uri.EscapeDataString(texto)}", Cancelamento);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal(0, fabrica.Mapas.Buscas);
    }

    [Fact]
    public async Task RecusaTextoLongoDemais()
    {
        var resposta = await _cliente.GetAsync($"/api/entrega/enderecos?texto={new string('a', 121)}", Cancelamento);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task EnderecoPertoDaLojaNaoTemPedidoMinimo()
    {
        fabrica.Mapas.Reiniciar();

        var resultado = await ConferirAsync(AoSul(900));

        Assert.Equal("sem-minimo", resultado.Situacao);
        Assert.InRange(resultado.DistanciaEmLinhaRetaEmMetros, 895, 905);
        Assert.InRange(resultado.DistanciaEmMetros!.Value, 1_160, 1_180);
        Assert.Equal(1, fabrica.Mapas.Trajetos);
    }

    [Fact]
    public async Task EnderecoQueSoPassaDoRaioPeloTrajetoTemPedidoMinimo()
    {
        fabrica.Mapas.Reiniciar();

        // 1,4 km em linha reta, mas 1,82 km pelas ruas.
        var resultado = await ConferirAsync(AoSul(1_400));

        Assert.Equal("com-minimo", resultado.Situacao);
        Assert.InRange(resultado.DistanciaEmMetros!.Value, 1_810, 1_830);
    }

    [Fact]
    public async Task EnderecoPertoDoLimiteFicaSemPedidoMinimo()
    {
        fabrica.Mapas.Reiniciar();

        var resultado = await ConferirAsync(AoSul(1_150));

        // 1,15 km em linha reta, 1,5 km pelas ruas: perto de 1,5 km, sem pedido mínimo.
        Assert.Equal("sem-minimo", resultado.Situacao);
    }

    [Fact]
    public async Task EnderecoBemLongeTemPedidoMinimoSemGastarCreditoComTrajeto()
    {
        fabrica.Mapas.Reiniciar();

        var resultado = await ConferirAsync(AoSul(2_500));

        Assert.Equal("com-minimo", resultado.Situacao);
        Assert.Null(resultado.DistanciaEmMetros);
        Assert.Equal(0, fabrica.Mapas.Trajetos);
    }

    [Fact]
    public async Task EnderecoAlemDoAlcanceDaConsultaPedeConfirmacaoPeloWhatsApp()
    {
        fabrica.Mapas.Reiniciar();

        var resultado = await ConferirAsync(AoSul(6_000));

        Assert.Equal("fora-da-regiao", resultado.Situacao);
        Assert.Equal(0, fabrica.Mapas.Trajetos);
    }

    [Fact]
    public async Task SemTrajetoAteOPontoValeALinhaReta()
    {
        fabrica.Mapas.Reiniciar();
        fabrica.Mapas.SemTrajeto = true;

        var resultado = await ConferirAsync(AoSul(800));

        Assert.Equal("sem-minimo", resultado.Situacao);
        Assert.Null(resultado.DistanciaEmMetros);
    }

    [Theory]
    [InlineData("latitude=91&longitude=0")]
    [InlineData("latitude=0&longitude=-181")]
    [InlineData("latitude=abc&longitude=0")]
    [InlineData("latitude=1")]
    public async Task RecusaCoordenadasInvalidas(string consulta)
    {
        var resposta = await _cliente.GetAsync($"/api/entrega/conferir?{consulta}", Cancelamento);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Theory]
    [InlineData("/api/entrega/enderecos?texto=afranio")]
    [InlineData("/api/entrega/conferir?latitude=-22.81&longitude=-42.98")]
    public async Task ComOMapaIndisponivelPedeParaFalarPeloWhatsApp(string caminho)
    {
        fabrica.Mapas.Reiniciar();
        fabrica.Mapas.Indisponivel = true;

        var resposta = await _cliente.GetAsync(caminho, Cancelamento);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, resposta.StatusCode);
        var problema = await resposta.Content.ReadFromJsonAsync<ProblemDetails>(Cancelamento);
        Assert.Contains("temporariamente indisponível", problema?.Title, StringComparison.Ordinal);
        Assert.Contains("WhatsApp", problema?.Title, StringComparison.Ordinal);

        fabrica.Mapas.Reiniciar();
    }

    [Fact]
    public async Task ConsultasDeEntregaNaoFicamEmCache()
    {
        fabrica.Mapas.Reiniciar();

        await _cliente.GetAsync("/api/entrega/enderecos?texto=afranio", Cancelamento);
        await _cliente.GetAsync("/api/entrega/enderecos?texto=afranio", Cancelamento);

        // Quem evita gastar crédito com texto repetido é o cache do cliente de mapas, não o do site.
        Assert.Equal(2, fabrica.Mapas.Buscas);
    }

    private async Task<ResultadoDaEntregaDto> ConferirAsync(double latitude)
    {
        var url = string.Create(CultureInfo.InvariantCulture, $"/api/entrega/conferir?latitude={latitude}&longitude={LongitudeDaLoja}");
        var resultado = await _cliente.GetFromJsonAsync<ResultadoDaEntregaDto>(url, Cancelamento);

        return resultado!;
    }

    /// <summary>Ponto na mesma longitude da loja, a tantos metros em linha reta ao sul.</summary>
    private static double AoSul(double metros) => LatitudeDaLoja - (metros / MetrosPorGrauDeLatitude);
}

public sealed class ApiComPoucasConsultasFactory : BorgonhaApiFactory
{
    protected override int ConsultasDeEntregaPorMinuto => 3;
}

public sealed class LimiteDeConsultasDeEntregaTests(ApiComPoucasConsultasFactory fabrica) : IClassFixture<ApiComPoucasConsultasFactory>
{
    [Fact]
    public async Task BarraQuemConsultaMuitoDepressaEExplicaOMotivo()
    {
        using var cliente = fabrica.CreateClient();
        var cancelamento = TestContext.Current.CancellationToken;

        var respostas = new List<HttpStatusCode>();
        for (var tentativa = 0; tentativa < 4; tentativa++)
        {
            respostas.Add((await cliente.GetAsync("/api/entrega/enderecos?texto=afranio", cancelamento)).StatusCode);
        }

        Assert.Equal(
            [HttpStatusCode.OK, HttpStatusCode.OK, HttpStatusCode.OK, HttpStatusCode.TooManyRequests],
            respostas);

        var barrada = await cliente.GetAsync("/api/entrega/enderecos?texto=afranio", cancelamento);
        Assert.True(barrada.Headers.Contains("Retry-After"));
        var problema = await barrada.Content.ReadFromJsonAsync<ProblemDetails>(cancelamento);
        Assert.Contains("Muitas consultas", problema?.Title, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ORestoDoSiteNaoEntraNoLimite()
    {
        using var cliente = fabrica.CreateClient();
        var cancelamento = TestContext.Current.CancellationToken;

        for (var tentativa = 0; tentativa < 8; tentativa++)
        {
            Assert.Equal(HttpStatusCode.OK, (await cliente.GetAsync("/api/duvidas", cancelamento)).StatusCode);
        }
    }
}
