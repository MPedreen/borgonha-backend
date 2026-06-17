namespace Borgonha.Service.Produtos.Dtos;

public sealed record CriarProdutoRequest(
    string Nome,
    decimal PrecoVenda,
    IReadOnlyCollection<ItemReceitaRequest> Receita);

public sealed record ItemReceitaRequest(Guid IngredienteId, decimal Quantidade);
