using System.Net;
using System.Net.Http.Json;

namespace Borgonha.Api.Tests;

/// <summary>
/// API com só 3 tentativas de login por endereço, para testar o freio contra força bruta.
/// </summary>
public sealed class ApiComPoucasTentativasFactory : BorgonhaApiFactory
{
    protected override int TentativasDeLogin => 3;
}

public sealed class LimiteDeTentativasDeLoginTests(ApiComPoucasTentativasFactory fabrica) : IClassFixture<ApiComPoucasTentativasFactory>
{
    [Fact]
    public async Task BloqueiaDepoisDoLimiteMesmoComASenhaCerta()
    {
        using var cliente = fabrica.CriarClienteHttps();
        cliente.DefaultRequestHeaders.Add("X-Borgonha-Admin", "1");

        var respostas = new List<HttpStatusCode>();
        foreach (var senha in new[] { "chute-1", "chute-2", "chute-3", BorgonhaApiFactory.SenhaAdmin })
        {
            var resposta = await cliente.PostAsJsonAsync(
                new Uri("/api/admin/sessao", UriKind.Relative),
                new { usuario = BorgonhaApiFactory.UsuarioAdmin, senha },
                TestContext.Current.CancellationToken);
            respostas.Add(resposta.StatusCode);
        }

        Assert.Equal(
            [HttpStatusCode.Unauthorized, HttpStatusCode.Unauthorized, HttpStatusCode.Unauthorized, HttpStatusCode.TooManyRequests],
            respostas);
    }
}
