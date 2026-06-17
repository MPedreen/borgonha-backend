using Borgonha.Domain.Entities;
using Borgonha.Domain.Repositories;
using Borgonha.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Borgonha.Infrastructure.Repositories;

internal sealed class IngredienteRepository(BorgonhaDbContext context) : IIngredienteRepository
{
    public Task<Ingrediente?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        => context.Ingredientes.FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Ingrediente>> ObterTodosAsync(CancellationToken cancellationToken = default)
        => await context.Ingredientes.ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Ingrediente>> ObterEmAlertaAsync(CancellationToken cancellationToken = default)
        => await context.Ingredientes
            .Where(i => i.QuantidadeAtual <= i.QuantidadeMinima)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Ingrediente>> ObterPorIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        => await context.Ingredientes
            .Where(i => ids.Contains(i.Id))
            .ToListAsync(cancellationToken);

    public async Task AdicionarAsync(Ingrediente ingrediente, CancellationToken cancellationToken = default)
        => await context.Ingredientes.AddAsync(ingrediente, cancellationToken);

    public void Atualizar(Ingrediente ingrediente)
        => context.Ingredientes.Update(ingrediente);
}
