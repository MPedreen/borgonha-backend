using Borgonha.Aplicacao.Catalogo;

namespace Borgonha.Aplicacao.Administracao;

/// <summary>
/// Coloca o produto no cardápio, marca como esgotado ou tira do site.
/// </summary>
public sealed class AlterarSituacaoDoProduto(ICatalogoRepositorio repositorio)
{
    /// <returns>O produto atualizado, ou <see langword="null"/> se ele não existir.</returns>
    public async Task<ProdutoAdminDto?> ExecutarAsync(string slug, SituacaoDoProdutoDto dados, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dados);

        var situacao = SituacoesDoProduto.Ler(dados.Situacao);
        var produto = await repositorio.ObterProdutoAsync(slug, cancellationToken).ConfigureAwait(false);
        if (produto is null)
        {
            return null;
        }

        produto.DefinirSituacao(situacao);
        await repositorio.SalvarAsync(cancellationToken).ConfigureAwait(false);

        return produto.ParaDtoAdmin(produto.Categoria.Slug);
    }
}
