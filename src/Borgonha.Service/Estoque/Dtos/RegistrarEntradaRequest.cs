namespace Borgonha.Service.Estoque.Dtos;

public sealed record RegistrarEntradaRequest(decimal Quantidade, string? Observacao);
