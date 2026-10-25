using Borgonha.Api.Configuracao;

namespace Borgonha.Api.Endpoints;

internal static class EndpointsDaApi
{
    public static IEndpointRouteBuilder MapearEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api").CacheOutput(PoliticasDeCache.Conteudo);

        api.MapearCatalogo();
        api.MapearLoja();
        api.MapearDuvidas();

        // Fora do grupo /api: a consulta de entrega e a área do administrador não passam pelo cache do site.
        app.MapearEntrega();

        app.MapGroup("/api/admin")
            .WithTags("Área do administrador")
            .AddEndpointFilter(ExigirCabecalhoDoAdministrador)
            .MapearSessaoAdmin()
            .MapearCardapioAdmin();

        app.MapHealthChecks("/health");

        return app;
    }

    /// <summary>
    /// Exige o cabeçalho da área do administrador (proteção contra CSRF) e impede que respostas
    /// com dados da área fiquem guardadas no navegador ou em proxies.
    /// </summary>
    private static async ValueTask<object?> ExigirCabecalhoDoAdministrador(EndpointFilterInvocationContext contexto, EndpointFilterDelegate proximo)
    {
        var http = contexto.HttpContext;
        http.Response.Headers.CacheControl = "no-store";

        if (!http.Request.Headers.ContainsKey(AutenticacaoDoAdministrador.Cabecalho))
        {
            return TypedResults.Problem(statusCode: StatusCodes.Status403Forbidden, title: "Requisição recusada.");
        }

        return await proximo(contexto).ConfigureAwait(false);
    }
}
