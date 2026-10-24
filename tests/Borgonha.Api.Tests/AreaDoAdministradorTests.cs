using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;

namespace Borgonha.Api.Tests;

public sealed class AreaDoAdministradorTests(BorgonhaApiFactory fabrica) : IClassFixture<BorgonhaApiFactory>
{
    private const string Cabecalho = "X-Borgonha-Admin";

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
}
