using Borgonha.Domain.Primitives;

namespace Borgonha.Domain.Entities;

public sealed class ItemVenda : Entity
{
    private ItemVenda() { }

    private ItemVenda(Guid id, Guid vendaId, Guid produtoId, int quantidade, decimal precoUnitario)
        : base(id)
    {
        VendaId = vendaId;
        ProdutoId = produtoId;
        Quantidade = quantidade;
        PrecoUnitario = precoUnitario;
    }

    public Guid VendaId { get; private set; }
    public Guid ProdutoId { get; private set; }
    public int Quantidade { get; private set; }
    public decimal PrecoUnitario { get; private set; }
    public decimal Subtotal => Quantidade * PrecoUnitario;

    internal static ItemVenda Criar(Guid vendaId, Guid produtoId, int quantidade, decimal precoUnitario)
        => new(Guid.NewGuid(), vendaId, produtoId, quantidade, precoUnitario);
}
