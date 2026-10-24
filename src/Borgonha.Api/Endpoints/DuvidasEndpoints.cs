using Borgonha.Aplicacao.Duvidas;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Borgonha.Api.Endpoints;

internal static class DuvidasEndpoints
{
    public static RouteGroupBuilder MapearDuvidas(this RouteGroupBuilder api)
    {
        api.MapGet("/duvidas", ListarDuvidasAsync)
            .WithTags("Dúvidas")
            .WithName("ListarDuvidas")
            .WithSummary("Perguntas frequentes, na ordem de exibição.");

        return api;
    }

    private static async Task<Ok<IReadOnlyList<DuvidaDto>>> ListarDuvidasAsync(ListarDuvidas consulta, CancellationToken cancellationToken) =>
        TypedResults.Ok(await consulta.ExecutarAsync(cancellationToken).ConfigureAwait(false));
}
