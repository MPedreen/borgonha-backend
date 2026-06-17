namespace Borgonha.Service.Estoque.Dtos;

public sealed record AtualizarIngredienteRequest(
    string Nome,
    string Unidade,
    decimal QuantidadeMinima,
    decimal CustoUnitario);
