namespace Borgonha.Domain.Models;

public record RelatorioDiario(
    int TotalVendas,
    decimal ReceitaBruta,
    decimal CustoTotal,
    decimal Lucro);
