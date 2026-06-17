namespace Borgonha.Service.Estoque.Dtos;

public sealed record IngredienteDto(
    Guid Id,
    string Nome,
    string Unidade,
    decimal QuantidadeAtual,
    decimal QuantidadeMinima,
    decimal CustoUnitario,
    DateTime CriadoEm,
    bool EstaEmAlerta);
