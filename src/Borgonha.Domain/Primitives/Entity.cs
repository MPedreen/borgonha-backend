namespace Borgonha.Domain.Primitives;

public abstract class Entity
{
    protected Entity(Guid id) => Id = id;
    protected Entity() { }

    public Guid Id { get; private set; }

    public override bool Equals(object? obj)
    {
        if (obj is not Entity outro || obj.GetType() != GetType())
            return false;

        return Id == outro.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();
}
