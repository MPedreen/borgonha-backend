namespace Borgonha.Service.Pdv.Dtos;

public sealed record RegistrarVendaRequest(IReadOnlyCollection<ItemVendaRequest> Itens, decimal ValorPago);

public sealed record ItemVendaRequest(Guid ProdutoId, int Quantidade);
