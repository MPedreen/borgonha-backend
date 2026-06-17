namespace Borgonha.Service.Pdv.Dtos;

public sealed record VendaDto(
    Guid Id,
    string CriadoPor,
    decimal Total,
    decimal ValorPago,
    decimal Troco,
    DateTime DataHora,
    IReadOnlyCollection<ItemVendaDto> Itens);

public sealed record ItemVendaDto(Guid ProdutoId, int Quantidade, decimal PrecoUnitario, decimal Subtotal);
