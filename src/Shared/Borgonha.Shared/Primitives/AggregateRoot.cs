namespace Borgonha.Shared.Primitives;

public abstract class AggregateRoot : Entity
{
    private readonly List<IDomainEvent> _eventos = [];

    protected AggregateRoot(Guid id) : base(id) { }
    protected AggregateRoot() { }

    public IReadOnlyCollection<IDomainEvent> GetEventosDominio() => _eventos.AsReadOnly();

    public void LimparEventosDominio() => _eventos.Clear();

    protected void AdicionarEventoDominio(IDomainEvent evento) => _eventos.Add(evento);
}
