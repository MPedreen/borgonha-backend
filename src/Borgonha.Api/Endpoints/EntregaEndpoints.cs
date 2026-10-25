using Borgonha.Api.Configuracao;
using Borgonha.Aplicacao.Entrega;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Borgonha.Api.Endpoints;

/// <summary>
/// Consulta de entrega: o cliente digita o endereço, escolhe uma sugestão e vê se o pedido tem valor mínimo.
/// Ficam fora do cache do site e atrás de um limite de consultas por endereço IP, porque cada chamada
/// gasta o limite diário do serviço de mapas.
/// </summary>
internal static class EntregaEndpoints
{
    public static IEndpointRouteBuilder MapearEntrega(this IEndpointRouteBuilder app)
    {
        var entrega = app.MapGroup("/api/entrega")
            .WithTags("Entrega")
            .RequireRateLimiting(LimiteDeConsultasDeEntrega.Politica);

        entrega.MapGet("/enderecos", BuscarEnderecosAsync)
            .WithName("BuscarEnderecosDeEntrega")
            .WithSummary("Sugere endereços a partir do texto digitado (mínimo de 4 letras). 503 se o serviço de mapas estiver indisponível.");

        entrega.MapGet("/conferir", ConferirAsync)
            .WithName("ConferirEntrega")
            .WithSummary("Diz se a entrega no ponto (latitude e longitude de uma sugestão) tem pedido mínimo. 503 se o serviço de mapas estiver indisponível.");

        return app;
    }

    private static async Task<Ok<IReadOnlyList<EnderecoSugeridoDto>>> BuscarEnderecosAsync(
        string? texto,
        BuscarEnderecosDeEntrega consulta,
        CancellationToken cancellationToken) =>
        TypedResults.Ok(await consulta.ExecutarAsync(texto, cancellationToken).ConfigureAwait(false));

    private static async Task<Ok<ResultadoDaEntregaDto>> ConferirAsync(
        double latitude,
        double longitude,
        ConferirEntrega consulta,
        CancellationToken cancellationToken) =>
        TypedResults.Ok(await consulta.ExecutarAsync(latitude, longitude, cancellationToken).ConfigureAwait(false));
}
