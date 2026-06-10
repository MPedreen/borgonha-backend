namespace Borgonha.Shared.Primitives;

public interface IDomainEvent
{
    Guid EventoId { get; }
    DateTime OcorridoEm { get; }
}
