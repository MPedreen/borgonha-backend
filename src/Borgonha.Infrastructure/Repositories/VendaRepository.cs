using Borgonha.Domain.Entities;
using Borgonha.Domain.Repositories;
using Borgonha.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Borgonha.Infrastructure.Repositories;

internal sealed class VendaRepository(BorgonhaDbContext context) : IVendaRepository
{
    public Task<Venda?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        => context.Vendas
            .Include(v => v.Itens)
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

    public async Task AdicionarAsync(Venda venda, CancellationToken cancellationToken = default)
        => await context.Vendas.AddAsync(venda, cancellationToken);
}
