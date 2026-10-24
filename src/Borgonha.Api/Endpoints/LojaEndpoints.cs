using Borgonha.Aplicacao.Lojas;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Borgonha.Api.Endpoints;

internal static class LojaEndpoints
{
    public static RouteGroupBuilder MapearLoja(this RouteGroupBuilder api)
    {
        api.MapGet("/loja", ObterLojaAsync)
            .WithTags("Loja")
            .WithName("ObterLoja")
            .WithSummary("Endereço, horários e canais de pedido (WhatsApp, iFood e loja física).");

        return api;
    }

    private static async Task<Results<Ok<LojaDto>, NotFound>> ObterLojaAsync(ObterLoja consulta, CancellationToken cancellationToken)
    {
        var loja = await consulta.ExecutarAsync(cancellationToken).ConfigureAwait(false);

        return loja is null ? TypedResults.NotFound() : TypedResults.Ok(loja);
    }
}
