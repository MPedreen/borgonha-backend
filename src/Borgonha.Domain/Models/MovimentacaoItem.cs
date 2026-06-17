using Borgonha.Domain.Enums;

namespace Borgonha.Domain.Models;

public record MovimentacaoItem(
    Guid Id,
    TipoMovimentacao Tipo,
    decimal Quantidade,
    DateTime DataHora,
    Guid? VendaId,
    string? Observacao);
