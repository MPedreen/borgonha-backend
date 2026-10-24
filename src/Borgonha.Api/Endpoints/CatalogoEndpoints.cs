using Borgonha.Aplicacao.Catalogo;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Borgonha.Api.Endpoints;

internal static class CatalogoEndpoints
{
    public static RouteGroupBuilder MapearCatalogo(this RouteGroupBuilder api)
    {
        var catalogo = api.MapGroup("/catalogo").WithTags("Catálogo");

        catalogo.MapGet("/", ObterCatalogoAsync)
            .WithName("ObterCatalogo")
            .WithSummary("Cardápio completo, agrupado por categoria e na ordem de exibição.");

        catalogo.MapGet("/{slug}", ObterProdutoAsync)
            .WithName("ObterProduto")
            .WithSummary("Detalhes de um produto.");

        return api;
    }

    private static async Task<Ok<CatalogoDto>> ObterCatalogoAsync(ObterCatalogo consulta, CancellationToken cancellationToken) =>
        TypedResults.Ok(await consulta.ExecutarAsync(cancellationToken).ConfigureAwait(false));

    private static async Task<Results<Ok<ProdutoDto>, NotFound>> ObterProdutoAsync(
        string slug,
        ObterProduto consulta,
        CancellationToken cancellationToken)
    {
        var produto = await consulta.ExecutarAsync(slug, cancellationToken).ConfigureAwait(false);

        return produto is null ? TypedResults.NotFound() : TypedResults.Ok(produto);
    }
}
