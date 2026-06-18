namespace Borgonha.Service.Produtos.Dtos;

public sealed record ProdutoDto(
    Guid Id,
    string Nome,
    decimal PrecoVenda,
    bool Ativo,
    bool Disponivel,
    DateTime CriadoEm,
    IReadOnlyCollection<ItemReceitaDto> Receita);

public sealed record ItemReceitaDto(Guid IngredienteId, decimal Quantidade);
