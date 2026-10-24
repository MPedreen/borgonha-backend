using Borgonha.Aplicacao.Catalogo;

namespace Borgonha.Aplicacao.Administracao;

/// <summary>
/// Apaga o produto de vez, junto com as fotos enviadas para ele.
/// Para tirar do site só por um tempo, use <see cref="AlterarSituacaoDoProduto"/>.
/// </summary>
public sealed class ExcluirProduto(ICatalogoRepositorio repositorio, IArmazenamentoDeImagens armazenamento)
{
    /// <returns><see langword="false"/> se o produto não existir.</returns>
    public async Task<bool> ExecutarAsync(string slug, CancellationToken cancellationToken)
    {
        var produto = await repositorio.ObterProdutoAsync(slug, cancellationToken).ConfigureAwait(false);
        if (produto is null)
        {
            return false;
        }

        repositorio.Remover(produto);
        await repositorio.SalvarAsync(cancellationToken).ConfigureAwait(false);

        foreach (var imagem in new[] { produto.ImagemInteiro, produto.ImagemPartido })
        {
            if (imagem is not null)
            {
                await armazenamento.RemoverAsync(imagem.Caminho, cancellationToken).ConfigureAwait(false);
            }
        }

        return true;
    }
}
