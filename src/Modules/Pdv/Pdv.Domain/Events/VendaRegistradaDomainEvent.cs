using Borgonha.Shared.Primitives;

namespace Pdv.Domain.Events;

public sealed record VendaRegistradaDomainEvent(
    Guid EventoId,
    DateTime OcorridoEm,
    Guid VendaId,
    string CriadoPor,
    decimal Total) : IDomainEvent
{
    public static VendaRegistradaDomainEvent Criar(Guid vendaId, string criadoPor, decimal total)
        => new(Guid.NewGuid(), DateTime.UtcNow, vendaId, criadoPor, total);
}
