using Borgonha.Domain.Primitives;

namespace Borgonha.Domain.Entities;

public sealed class ReceitaItem : Entity
{
    private ReceitaItem() { }

    private ReceitaItem(Guid id, Guid produtoId, Guid ingredienteId, decimal quantidade)
        : base(id)
    {
        ProdutoId = produtoId;
        IngredienteId = ingredienteId;
        Quantidade = quantidade;
    }

    public Guid ProdutoId { get; private set; }
    public Guid IngredienteId { get; private set; }
    public decimal Quantidade { get; private set; }

    internal static ReceitaItem Criar(Guid produtoId, Guid ingredienteId, decimal quantidade)
        => new(Guid.NewGuid(), produtoId, ingredienteId, quantidade);
}
