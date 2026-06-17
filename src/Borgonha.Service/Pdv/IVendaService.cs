using Borgonha.Domain.Primitives;
using Borgonha.Service.Pdv.Dtos;

namespace Borgonha.Service.Pdv;

public interface IVendaService
{
    Task<Result<VendaDto>> RegistrarAsync(string criadoPor, RegistrarVendaRequest request, CancellationToken cancellationToken = default);
    Task<Result<VendaDto>> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
}
