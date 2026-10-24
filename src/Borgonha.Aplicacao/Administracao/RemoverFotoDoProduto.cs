using Borgonha.Aplicacao.Catalogo;

namespace Borgonha.Aplicacao.Administracao;

/// <summary>
/// Tira uma foto do produto. Sem a foto inteira, o site mostra um espaço reservado;
/// sem a foto partida, o produto sai da galeria "por dentro".
/// </summary>
public sealed class RemoverFotoDoProduto(ICatalogoRepositorio repositorio, IArmazenamentoDeImagens armazenamento)
{
    /// <returns>O produto atualizado, ou <see langword="null"/> se ele não existir.</returns>
    public async Task<ProdutoAdminDto?> ExecutarAsync(string slug, TipoDeFoto tipo, CancellationToken cancellationToken)
    {
        var produto = await repositorio.ObterProdutoAsync(slug, cancellationToken).ConfigureAwait(false);
        if (produto is null)
        {
            return null;
        }

        var anterior = tipo == TipoDeFoto.Inteiro ? produto.ImagemInteiro : produto.ImagemPartido;
        if (tipo == TipoDeFoto.Inteiro)
        {
            produto.TrocarImagemInteiro(null);
        }
        else
        {
            produto.TrocarImagemPartido(null);
        }

        await repositorio.SalvarAsync(cancellationToken).ConfigureAwait(false);

        if (anterior is not null)
        {
            await armazenamento.RemoverAsync(anterior.Caminho, cancellationToken).ConfigureAwait(false);
        }

        return produto.ParaDtoAdmin(produto.Categoria.Slug);
    }
}
