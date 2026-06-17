using Borgonha.Domain.Primitives;
using Borgonha.Service.Estoque.Dtos;

namespace Borgonha.Service.Estoque;

public interface IIngredienteService
{
    Task<Result<IngredienteDto>> CriarAsync(CriarIngredienteRequest request, CancellationToken cancellationToken = default);
    Task<Result<IngredienteDto>> AtualizarAsync(Guid id, AtualizarIngredienteRequest request, CancellationToken cancellationToken = default);
    Task<Result<IngredienteDto>> RegistrarEntradaAsync(Guid id, RegistrarEntradaRequest request, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<IngredienteDto>>> ObterTodosAsync(CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<IngredienteDto>>> ObterEmAlertaAsync(CancellationToken cancellationToken = default);
}
