namespace Borgonha.Domain.Models;

public record PaginaMovimentacoes(
    int Total,
    int Pagina,
    int Tamanho,
    IReadOnlyList<MovimentacaoItem> Itens);
