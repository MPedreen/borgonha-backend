using Borgonha.Dominio.Comum;

namespace Borgonha.Aplicacao.Catalogo;

public sealed class ObterProduto(ICatalogoRepositorio repositorio)
{
    /// <returns>
    /// O produto, ou <see langword="null"/> se o slug for inválido, não existir ou o produto estiver fora do cardápio.
    /// </returns>
    public async Task<ProdutoDto?> ExecutarAsync(string slug, CancellationToken cancellationToken)
    {
        if (!Slug.TentarCriar(slug, out var slugValido))
        {
            return null;
        }

        var produto = await repositorio.ObterProdutoAsync(slugValido, cancellationToken).ConfigureAwait(false);

        return produto is { Ativo: true } ? produto.ParaDto(produto.Categoria.Slug) : null;
    }
}
