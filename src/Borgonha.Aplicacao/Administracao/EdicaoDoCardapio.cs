using Borgonha.Aplicacao.Catalogo;
using Borgonha.Dominio.Catalogo;
using Borgonha.Dominio.Comum;

namespace Borgonha.Aplicacao.Administracao;

/// <summary>
/// Leitura de um produto para edição a partir do slug vindo da URL.
/// </summary>
internal static class EdicaoDoCardapio
{
    /// <returns>O produto, ou <see langword="null"/> se o slug for inválido ou não existir.</returns>
    public static async Task<Produto?> ObterProdutoAsync(
        this ICatalogoRepositorio repositorio,
        string slug,
        CancellationToken cancellationToken) =>
        Slug.TentarCriar(slug, out var slugValido)
            ? await repositorio.ObterProdutoParaEdicaoAsync(slugValido, cancellationToken).ConfigureAwait(false)
            : null;
}
