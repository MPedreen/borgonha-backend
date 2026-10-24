using Borgonha.Api.Configuracao;
using Borgonha.Aplicacao.Administracao;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace Borgonha.Api.Endpoints;

/// <summary>
/// Edição do cardápio pela área do administrador. Toda alteração limpa o cache do site,
/// para o cliente ver o cardápio do dia na hora.
/// </summary>
internal static class CardapioAdminEndpoints
{
    /// <summary>Um pouco acima do limite da foto (15 MB), para caber o resto do formulário.</summary>
    private const long TamanhoMaximoDoEnvio = 16 * 1024 * 1024;

    public static RouteGroupBuilder MapearCardapioAdmin(this RouteGroupBuilder admin)
    {
        var cardapio = admin.MapGroup("/").RequireAuthorization(AutenticacaoDoAdministrador.Politica);

        cardapio.MapGet("/cardapio", ListarAsync)
            .WithName("ListarCardapioAdmin")
            .WithSummary("Todos os produtos, inclusive os ocultos, com a situação de cada um.");

        var produtos = cardapio.MapGroup("/produtos").AddEndpointFilter(LimparCacheDoSite);

        produtos.MapPost("/", CriarAsync)
            .WithName("CriarProduto")
            .WithSummary("Cadastra um produto novo no fim da categoria.");

        produtos.MapPut("/{slug}", AtualizarAsync)
            .WithName("AtualizarProduto")
            .WithSummary("Troca nome, descrição, frase, gramatura e variações.");

        produtos.MapPut("/{slug}/situacao", AlterarSituacaoAsync)
            .WithName("AlterarSituacaoDoProduto")
            .WithSummary("no-cardapio, esgotado (aparece com o aviso) ou oculto (sai do site).");

        produtos.MapPut("/{slug}/fotos/{tipo}", TrocarFotoAsync)
            // Formulário de arquivo sem token antiforgery: o CSRF é barrado pelo cookie SameSite=Strict
            // e pelo cabeçalho obrigatório da área do administrador.
            .DisableAntiforgery()
            .WithMetadata(new RequestSizeLimitAttribute(TamanhoMaximoDoEnvio))
            .WithName("TrocarFotoDoProduto")
            .WithSummary("Envia a foto do produto inteiro ou partido (JPG, PNG, WebP ou HEIC, até 15 MB).");

        produtos.MapDelete("/{slug}/fotos/{tipo}", RemoverFotoAsync)
            .WithName("RemoverFotoDoProduto")
            .WithSummary("Tira a foto do produto inteiro ou partido.");

        produtos.MapDelete("/{slug}", ExcluirAsync)
            .WithName("ExcluirProduto")
            .WithSummary("Apaga o produto de vez (para tirar só por um tempo, use a situação \"oculto\").");

        return admin;
    }

    private static async Task<Ok<CardapioAdminDto>> ListarAsync(ListarCardapioAdmin consulta, CancellationToken cancellationToken) =>
        TypedResults.Ok(await consulta.ExecutarAsync(cancellationToken).ConfigureAwait(false));

    private static async Task<Created<ProdutoAdminDto>> CriarAsync(NovoProdutoDto dados, CriarProduto caso, CancellationToken cancellationToken)
    {
        var produto = await caso.ExecutarAsync(dados, cancellationToken).ConfigureAwait(false);
        return TypedResults.Created($"/api/admin/produtos/{produto.Slug}", produto);
    }

    private static async Task<Results<Ok<ProdutoAdminDto>, NotFound>> AtualizarAsync(
        string slug,
        DadosDoProdutoDto dados,
        AtualizarProduto caso,
        CancellationToken cancellationToken) =>
        Resultado(await caso.ExecutarAsync(slug, dados, cancellationToken).ConfigureAwait(false));

    private static async Task<Results<Ok<ProdutoAdminDto>, NotFound>> AlterarSituacaoAsync(
        string slug,
        SituacaoDoProdutoDto dados,
        AlterarSituacaoDoProduto caso,
        CancellationToken cancellationToken) =>
        Resultado(await caso.ExecutarAsync(slug, dados, cancellationToken).ConfigureAwait(false));

    private static async Task<Results<Ok<ProdutoAdminDto>, NotFound>> TrocarFotoAsync(
        string slug,
        string tipo,
        IFormFile foto,
        TrocarFotoDoProduto caso,
        CancellationToken cancellationToken)
    {
        var tipoDeFoto = TiposDeFoto.Ler(tipo);
        var conteudo = foto.OpenReadStream();
        await using (conteudo.ConfigureAwait(false))
        {
            return Resultado(await caso.ExecutarAsync(slug, tipoDeFoto, conteudo, cancellationToken).ConfigureAwait(false));
        }
    }

    private static async Task<Results<Ok<ProdutoAdminDto>, NotFound>> RemoverFotoAsync(
        string slug,
        string tipo,
        RemoverFotoDoProduto caso,
        CancellationToken cancellationToken) =>
        Resultado(await caso.ExecutarAsync(slug, TiposDeFoto.Ler(tipo), cancellationToken).ConfigureAwait(false));

    private static async Task<Results<NoContent, NotFound>> ExcluirAsync(string slug, ExcluirProduto caso, CancellationToken cancellationToken) =>
        await caso.ExecutarAsync(slug, cancellationToken).ConfigureAwait(false) ? TypedResults.NoContent() : TypedResults.NotFound();

    private static Results<Ok<ProdutoAdminDto>, NotFound> Resultado(ProdutoAdminDto? produto) =>
        produto is null ? TypedResults.NotFound() : TypedResults.Ok(produto);

    /// <summary>
    /// Depois de uma alteração bem-sucedida, descarta o cardápio guardado em cache.
    /// </summary>
    private static async ValueTask<object?> LimparCacheDoSite(EndpointFilterInvocationContext contexto, EndpointFilterDelegate proximo)
    {
        var resultado = await proximo(contexto).ConfigureAwait(false);

        // Results<Ok<T>, NotFound> embrulha o resultado de verdade, que é quem tem o status.
        var resultadoFinal = resultado is INestedHttpResult embrulhado ? embrulhado.Result : resultado;

        if (resultadoFinal is IStatusCodeHttpResult { StatusCode: < 400 })
        {
            var cache = contexto.HttpContext.RequestServices.GetRequiredService<IOutputCacheStore>();
            await cache.EvictByTagAsync(PoliticasDeCache.Conteudo, contexto.HttpContext.RequestAborted).ConfigureAwait(false);
        }

        return resultado;
    }
}
