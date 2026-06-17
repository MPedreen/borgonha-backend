using Borgonha.Domain.Entities;

namespace Borgonha.Domain.Repositories;

public interface IIngredienteRepository
{
    Task<Ingrediente?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Ingrediente>> ObterTodosAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Ingrediente>> ObterEmAlertaAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Ingrediente>> ObterPorIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Ingrediente ingrediente, CancellationToken cancellationToken = default);
    void Atualizar(Ingrediente ingrediente);
}
