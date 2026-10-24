using Borgonha.Aplicacao.Catalogo;

namespace Borgonha.Aplicacao.Administracao;

/// <summary>
/// Troca nome, descrição, frase, gramatura, preço e variações de um produto.
/// </summary>
public sealed class AtualizarProduto(ICatalogoRepositorio repositorio)
{
    /// <returns>O produto atualizado, ou <see langword="null"/> se ele não existir.</returns>
    public async Task<ProdutoAdminDto?> ExecutarAsync(string slug, DadosDoProdutoDto dados, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dados);

        var produto = await repositorio.ObterProdutoAsync(slug, cancellationToken).ConfigureAwait(false);
        if (produto is null)
        {
            return null;
        }

        var nomeAnterior = produto.Nome;
        produto.Atualizar(dados.Nome, dados.Descricao, dados.Frase, dados.Peso, dados.Variacoes);
        produto.DefinirPreco(dados.Preco);

        if (produto.Nome != nomeAnterior)
        {
            produto.AtualizarTextosAlternativos();
        }

        await repositorio.SalvarAsync(cancellationToken).ConfigureAwait(false);

        return produto.ParaDtoAdmin(produto.Categoria.Slug);
    }
}
