using Borgonha.Shared.Primitives;
using Estoque.Domain.Enums;

namespace Estoque.Domain.Entities;

public sealed class MovimentacaoEstoque : Entity
{
    private MovimentacaoEstoque() { }

    private MovimentacaoEstoque(
        Guid id,
        Guid ingredienteId,
        TipoMovimentacao tipo,
        decimal quantidade,
        DateTime dataHora,
        Guid? vendaId,
        string? observacao)
        : base(id)
    {
        IngredienteId = ingredienteId;
        Tipo = tipo;
        Quantidade = quantidade;
        DataHora = dataHora;
        VendaId = vendaId;
        Observacao = observacao;
    }

    public Guid IngredienteId { get; private set; }
    public TipoMovimentacao Tipo { get; private set; }
    public decimal Quantidade { get; private set; }
    public DateTime DataHora { get; private set; }
    public Guid? VendaId { get; private set; }
    public string? Observacao { get; private set; }

    internal static MovimentacaoEstoque CriarSaida(Guid ingredienteId, decimal quantidade, Guid vendaId)
        => new(Guid.NewGuid(), ingredienteId, TipoMovimentacao.Saida, quantidade, DateTime.UtcNow, vendaId, null);

    internal static MovimentacaoEstoque CriarEntrada(Guid ingredienteId, decimal quantidade, string? observacao)
        => new(Guid.NewGuid(), ingredienteId, TipoMovimentacao.Entrada, quantidade, DateTime.UtcNow, null, observacao);
}
