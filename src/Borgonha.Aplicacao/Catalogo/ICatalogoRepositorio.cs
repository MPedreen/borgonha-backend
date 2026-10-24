using Borgonha.Dominio.Catalogo;
using Borgonha.Dominio.Comum;

namespace Borgonha.Aplicacao.Catalogo;

public interface ICatalogoRepositorio
{
    /// <summary>
    /// Categorias na ordem de exibição, com todos os produtos carregados (inclusive os ocultos).
    /// </summary>
    Task<IReadOnlyList<Categoria>> ListarCategoriasComProdutosAsync(CancellationToken cancellationToken);

    Task<Produto?> ObterProdutoAsync(Slug slug, CancellationToken cancellationToken);

    /// <summary>
    /// Produto com a categoria, acompanhado para ser alterado e salvo com <see cref="SalvarAsync"/>.
    /// </summary>
    Task<Produto?> ObterProdutoParaEdicaoAsync(Slug slug, CancellationToken cancellationToken);

    /// <summary>
    /// Categoria com os produtos, acompanhada para receber um produto novo.
    /// </summary>
    Task<Categoria?> ObterCategoriaParaEdicaoAsync(Slug slug, CancellationToken cancellationToken);

    Task<bool> ExisteProdutoComSlugAsync(Slug slug, CancellationToken cancellationToken);

    void Remover(Produto produto);

    Task SalvarAsync(CancellationToken cancellationToken);
}
