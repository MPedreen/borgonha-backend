using Borgonha.Domain.Errors;
using Borgonha.Domain.Primitives;

namespace Borgonha.Domain.Entities;

public sealed class Venda : Entity
{
    private readonly List<ItemVenda> _itens = [];

    private Venda() { }

    private Venda(Guid id, string criadoPor, decimal total, decimal valorPago, decimal troco, DateTime dataHora)
        : base(id)
    {
        CriadoPor = criadoPor;
        Total = total;
        ValorPago = valorPago;
        Troco = troco;
        DataHora = dataHora;
    }

    public string CriadoPor { get; private set; } = string.Empty;
    public decimal Total { get; private set; }
    public decimal ValorPago { get; private set; }
    public decimal Troco { get; private set; }
    public DateTime DataHora { get; private set; }
    public IReadOnlyCollection<ItemVenda> Itens => _itens.AsReadOnly();

    public static Result<Venda> Criar(
        string criadoPor,
        decimal valorPago,
        IEnumerable<(Guid ProdutoId, int Quantidade, decimal PrecoUnitario)> itens)
    {
        if (string.IsNullOrWhiteSpace(criadoPor))
            return Result.Falha<Venda>(PdvErrors.Venda.CriadoPorObrigatorio);

        var listaItens = itens.ToList();

        if (listaItens.Count == 0)
            return Result.Falha<Venda>(PdvErrors.Venda.SemItens);

        if (listaItens.Any(i => i.Quantidade <= 0))
            return Result.Falha<Venda>(PdvErrors.Item.QuantidadeInvalida);

        if (listaItens.Any(i => i.PrecoUnitario <= 0))
            return Result.Falha<Venda>(PdvErrors.Item.PrecoInvalido);

        if (listaItens.GroupBy(i => i.ProdutoId).Any(g => g.Count() > 1))
            return Result.Falha<Venda>(PdvErrors.Item.ProdutoDuplicado);

        var total = listaItens.Sum(i => i.Quantidade * i.PrecoUnitario);

        if (valorPago < total)
            return Result.Falha<Venda>(PdvErrors.Venda.ValorPagoInsuficiente);

        var troco = valorPago - total;
        var id = Guid.NewGuid();

        var venda = new Venda(id, criadoPor.Trim(), total, valorPago, troco, DateTime.UtcNow);

        foreach (var (produtoId, quantidade, preco) in listaItens)
            venda._itens.Add(ItemVenda.Criar(id, produtoId, quantidade, preco));

        return Result.Ok(venda);
    }
}
