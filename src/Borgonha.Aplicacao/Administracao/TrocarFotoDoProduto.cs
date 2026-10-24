using Borgonha.Aplicacao.Catalogo;
using Borgonha.Dominio.Comum;

namespace Borgonha.Aplicacao.Administracao;

/// <summary>
/// Troca a foto do produto inteiro ou partido por uma foto enviada pelo administrador.
/// </summary>
public sealed class TrocarFotoDoProduto(ICatalogoRepositorio repositorio, IArmazenamentoDeImagens armazenamento)
{
    /// <returns>O produto atualizado, ou <see langword="null"/> se ele não existir.</returns>
    /// <exception cref="ImagemInvalidaException">O arquivo não é uma foto aceita.</exception>
    public async Task<ProdutoAdminDto?> ExecutarAsync(string slug, TipoDeFoto tipo, Stream foto, CancellationToken cancellationToken)
    {
        var produto = await repositorio.ObterProdutoAsync(slug, cancellationToken).ConfigureAwait(false);
        if (produto is null)
        {
            return null;
        }

        var anterior = tipo == TipoDeFoto.Inteiro ? produto.ImagemInteiro : produto.ImagemPartido;
        var caminho = await armazenamento.SalvarAsync(foto, cancellationToken).ConfigureAwait(false);
        var nova = new Imagem(caminho, produto.TextoAlternativo(tipo));

        try
        {
            if (tipo == TipoDeFoto.Inteiro)
            {
                produto.TrocarImagemInteiro(nova);
            }
            else
            {
                produto.TrocarImagemPartido(nova);
            }

            await repositorio.SalvarAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            // Sem o produto salvo, a foto nova ficaria perdida no disco.
            await armazenamento.RemoverAsync(caminho, CancellationToken.None).ConfigureAwait(false);
            throw;
        }

        if (anterior is not null)
        {
            await armazenamento.RemoverAsync(anterior.Caminho, cancellationToken).ConfigureAwait(false);
        }

        return produto.ParaDtoAdmin(produto.Categoria.Slug);
    }
}
