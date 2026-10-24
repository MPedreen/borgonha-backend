using Borgonha.Aplicacao.Catalogo;
using Borgonha.Dominio.Catalogo;
using Borgonha.Dominio.Comum;
using Borgonha.Infraestrutura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Borgonha.Infraestrutura.Repositorios;

internal sealed class CatalogoRepositorio(BorgonhaDbContext contexto) : ICatalogoRepositorio
{
    public async Task<IReadOnlyList<Categoria>> ListarCategoriasComProdutosAsync(CancellationToken cancellationToken) =>
        await contexto.Categorias
            .AsNoTracking()
            .Include(categoria => categoria.Produtos)
            .OrderBy(categoria => categoria.Ordem)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public Task<Produto?> ObterProdutoAsync(Slug slug, CancellationToken cancellationToken) =>
        contexto.Produtos
            .AsNoTracking()
            .Include(produto => produto.Categoria)
            .FirstOrDefaultAsync(produto => produto.Slug == slug, cancellationToken);

    public Task<Produto?> ObterProdutoParaEdicaoAsync(Slug slug, CancellationToken cancellationToken) =>
        contexto.Produtos
            .Include(produto => produto.Categoria)
            .FirstOrDefaultAsync(produto => produto.Slug == slug, cancellationToken);

    public Task<Categoria?> ObterCategoriaParaEdicaoAsync(Slug slug, CancellationToken cancellationToken) =>
        contexto.Categorias
            .Include(categoria => categoria.Produtos)
            .FirstOrDefaultAsync(categoria => categoria.Slug == slug, cancellationToken);

    public Task<bool> ExisteProdutoComSlugAsync(Slug slug, CancellationToken cancellationToken) =>
        contexto.Produtos.AnyAsync(produto => produto.Slug == slug, cancellationToken);

    public void Remover(Produto produto) => contexto.Produtos.Remove(produto);

    public Task SalvarAsync(CancellationToken cancellationToken) => contexto.SaveChangesAsync(cancellationToken);
}
