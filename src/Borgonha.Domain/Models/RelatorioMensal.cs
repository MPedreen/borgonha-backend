namespace Borgonha.Domain.Models;

public record RelatorioMensal(
    int TotalVendas,
    decimal ReceitaBruta,
    decimal CustoTotal,
    decimal Lucro,
    IReadOnlyList<RankingProduto> Ranking);
