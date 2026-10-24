using Borgonha.Aplicacao.Catalogo;
using Borgonha.Dominio.Catalogo;
using Borgonha.Dominio.Comum;

namespace Borgonha.Aplicacao.Administracao;

/// <summary>
/// Cadastra um produto novo no fim da categoria, já no cardápio. O slug sai do nome.
/// Só o nome é obrigatório: descrição, gramatura e preço podem vir depois.
/// </summary>
public sealed class CriarProduto(ICatalogoRepositorio repositorio)
{
    public async Task<ProdutoAdminDto> ExecutarAsync(NovoProdutoDto dados, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dados);

        var categoria = Slug.TentarCriar(dados.Categoria, out var slugDaCategoria)
            ? await repositorio.ObterCategoriaParaEdicaoAsync(slugDaCategoria, cancellationToken).ConfigureAwait(false)
            : null;

        if (categoria is null)
        {
            throw new DominioException("Categoria não encontrada.");
        }

        var slug = await GerarSlugLivreAsync(dados.Nome, cancellationToken).ConfigureAwait(false);
        var produto = Produto.Criar(slug, dados.Nome, dados.Descricao, categoria.ProximaOrdem(), dados.Frase, dados.Peso, dados.Variacoes);
        produto.DefinirPreco(dados.Preco);
        categoria.AdicionarProduto(produto);

        await repositorio.SalvarAsync(cancellationToken).ConfigureAwait(false);

        return produto.ParaDtoAdmin(categoria.Slug);
    }

    private async Task<Slug> GerarSlugLivreAsync(string? nome, CancellationToken cancellationToken)
    {
        var slugBase = Slug.GerarAPartirDe(nome ?? string.Empty);
        var slug = slugBase;

        for (var numero = 2; await repositorio.ExisteProdutoComSlugAsync(slug, cancellationToken).ConfigureAwait(false); numero++)
        {
            slug = slugBase.ComSufixo(numero);
        }

        return slug;
    }
}
