using Pdv.Domain.Entities;

namespace Pdv.Domain.Repositories;

public interface IVendaRepository
{
    Task<Venda?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Venda venda, CancellationToken cancellationToken = default);
}
