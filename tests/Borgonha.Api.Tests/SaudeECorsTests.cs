using System.Net;

namespace Borgonha.Api.Tests;

public sealed class SaudeECorsTests(BorgonhaApiFactory fabrica) : IClassFixture<BorgonhaApiFactory>
{
    private const string CabecalhoCors = "Access-Control-Allow-Origin";

    private readonly HttpClient _cliente = fabrica.CreateClient();

    [Fact]
    public async Task HealthCheckIndicaApiEBancoSaudaveis()
    {
        var resposta = await _cliente.GetAsync(new Uri("/health", UriKind.Relative), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Equal("Healthy", await resposta.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task LiberaChamadasDoSite()
    {
        var resposta = await RequisitarComOrigemAsync(BorgonhaApiFactory.OrigemDoSite);

        Assert.True(resposta.Headers.TryGetValues(CabecalhoCors, out var origens));
        Assert.Equal(BorgonhaApiFactory.OrigemDoSite, Assert.Single(origens));
    }

    [Fact]
    public async Task BloqueiaChamadasDeOutrasOrigens()
    {
        var resposta = await RequisitarComOrigemAsync("https://outro-site.test");

        Assert.False(resposta.Headers.Contains(CabecalhoCors));
    }

    private async Task<HttpResponseMessage> RequisitarComOrigemAsync(string origem)
    {
        using var requisicao = new HttpRequestMessage(HttpMethod.Get, new Uri("/api/loja", UriKind.Relative));
        requisicao.Headers.Add("Origin", origem);

        return await _cliente.SendAsync(requisicao, TestContext.Current.CancellationToken);
    }
}
