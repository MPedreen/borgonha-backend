namespace Borgonha.Service.Estoque.Dtos;

public sealed record CriarIngredienteRequest(
    string Nome,
    string Unidade,
    decimal QuantidadeAtual,
    decimal QuantidadeMinima,
    decimal CustoUnitario);
