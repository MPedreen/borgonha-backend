using System.Net.Http.Json;
using Borgonha.Aplicacao.Duvidas;

namespace Borgonha.Api.Tests;

public sealed class DuvidasEndpointsTests(BorgonhaApiFactory fabrica) : IClassFixture<BorgonhaApiFactory>
{
    private readonly HttpClient _cliente = fabrica.CreateClient();

    [Fact]
    public async Task RetornaAsDuvidasNaOrdemDeExibicao()
    {
        var duvidas = await _cliente.GetFromJsonAsync<List<DuvidaDto>>("/api/duvidas", TestContext.Current.CancellationToken);

        Assert.NotNull(duvidas);
        Assert.Equal(5, duvidas.Count);
        Assert.Equal("Como faço meu pedido?", duvidas[0].Pergunta);
        Assert.Equal("Tem pedido mínimo para entrega?", duvidas[2].Pergunta);
        Assert.All(duvidas, duvida => Assert.False(string.IsNullOrWhiteSpace(duvida.Resposta)));
    }
}
