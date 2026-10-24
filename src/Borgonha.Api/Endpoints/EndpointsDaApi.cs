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

        app.MapHealthChecks("/health");

        return app;
    }
}
