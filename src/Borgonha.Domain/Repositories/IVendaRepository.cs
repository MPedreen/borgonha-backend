using Borgonha.Domain.Entities;

namespace Borgonha.Domain.Repositories;

public interface IVendaRepository
{
    Task<Venda?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Venda venda, CancellationToken cancellationToken = default);
}
