using Borgonha.Domain.Entities;

namespace Borgonha.Domain.Repositories;

public interface IProdutoRepository
{
    Task<Produto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Produto>> ObterAtivosAsync(CancellationToken cancellationToken = default);
    Task AdicionarAsync(Produto produto, CancellationToken cancellationToken = default);
    void Atualizar(Produto produto);
}
