using Borgonha.Domain.Entities;
using Borgonha.Domain.Repositories;
using Borgonha.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Borgonha.Infrastructure.Repositories;

internal sealed class ProdutoRepository(BorgonhaDbContext context) : IProdutoRepository
{
    public Task<Produto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        => context.Produtos
            .Include(p => p.Receita)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Produto>> ObterAtivosAsync(CancellationToken cancellationToken = default)
        => await context.Produtos
            .Where(p => p.Ativo)
            .ToListAsync(cancellationToken);

    public async Task AdicionarAsync(Produto produto, CancellationToken cancellationToken = default)
        => await context.Produtos.AddAsync(produto, cancellationToken);

    public void Atualizar(Produto produto)
        => context.Produtos.Update(produto);
}
