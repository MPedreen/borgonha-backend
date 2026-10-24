using Borgonha.Dominio.Catalogo;
using Borgonha.Dominio.Comum;

namespace Borgonha.Aplicacao.Catalogo;

public interface ICatalogoRepositorio
{
    /// <summary>
    /// Categorias na ordem de exibição, com os respectivos produtos carregados.
    /// </summary>
    Task<IReadOnlyList<Categoria>> ListarCategoriasComProdutosAsync(CancellationToken cancellationToken);

    Task<Produto?> ObterProdutoAsync(Slug slug, CancellationToken cancellationToken);
}
