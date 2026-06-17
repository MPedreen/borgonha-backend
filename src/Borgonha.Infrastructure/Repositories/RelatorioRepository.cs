using Borgonha.Domain.Models;
using Borgonha.Domain.Repositories;
using Borgonha.Infrastructure.Dapper;
using Dapper;

namespace Borgonha.Infrastructure.Repositories;

internal sealed class RelatorioRepository(ConnectionFactory connectionFactory) : IRelatorioRepository
{
    public async Task<RelatorioDiario> ObterDiarioAsync(DateOnly data, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                COUNT(DISTINCT v.id)                                                        AS TotalVendas,
                COALESCE(SUM(v.total), 0)                                                   AS ReceitaBruta,
                COALESCE(SUM(iv.quantidade * ri.quantidade * i.custo_unitario), 0)          AS CustoTotal,
                COALESCE(SUM(v.total), 0)
                    - COALESCE(SUM(iv.quantidade * ri.quantidade * i.custo_unitario), 0)    AS Lucro
            FROM vendas v
            JOIN itens_venda iv ON iv.venda_id = v.id
            JOIN receita_itens ri ON ri.produto_id = iv.produto_id
            JOIN ingredientes i ON i.id = ri.ingrediente_id
            WHERE v.data_hora::date = @Data
            """;

        using var conn = connectionFactory.Criar();
        var row = await conn.QuerySingleAsync<DapperKpis>(sql, new { Data = data.ToDateTime(TimeOnly.MinValue) });
        return new RelatorioDiario((int)row.TotalVendas, row.ReceitaBruta, row.CustoTotal, row.Lucro);
    }

    public async Task<RelatorioMensal> ObterMensalAsync(int ano, int mes, CancellationToken cancellationToken = default)
    {
        const string sqlKpis = """
            SELECT
                COUNT(DISTINCT v.id)                                                        AS TotalVendas,
                COALESCE(SUM(v.total), 0)                                                   AS ReceitaBruta,
                COALESCE(SUM(iv.quantidade * ri.quantidade * i.custo_unitario), 0)          AS CustoTotal,
                COALESCE(SUM(v.total), 0)
                    - COALESCE(SUM(iv.quantidade * ri.quantidade * i.custo_unitario), 0)    AS Lucro
            FROM vendas v
            JOIN itens_venda iv ON iv.venda_id = v.id
            JOIN receita_itens ri ON ri.produto_id = iv.produto_id
            JOIN ingredientes i ON i.id = ri.ingrediente_id
            WHERE EXTRACT(YEAR FROM v.data_hora) = @Ano
              AND EXTRACT(MONTH FROM v.data_hora) = @Mes
            """;

        const string sqlRanking = """
            SELECT
                p.nome                                 AS Nome,
                SUM(iv.quantidade)                     AS UnidadesVendidas,
                SUM(iv.quantidade * iv.preco_unitario) AS Receita
            FROM itens_venda iv
            JOIN vendas v ON v.id = iv.venda_id
            JOIN produtos p ON p.id = iv.produto_id
            WHERE EXTRACT(YEAR FROM v.data_hora) = @Ano
              AND EXTRACT(MONTH FROM v.data_hora) = @Mes
            GROUP BY p.id, p.nome
            ORDER BY UnidadesVendidas DESC
            """;

        using var conn = connectionFactory.Criar();
        var kpis = await conn.QuerySingleAsync<DapperKpis>(sqlKpis, new { Ano = ano, Mes = mes });
        var ranking = (await conn.QueryAsync<DapperRanking>(sqlRanking, new { Ano = ano, Mes = mes })).ToList();

        return new RelatorioMensal(
            (int)kpis.TotalVendas,
            kpis.ReceitaBruta,
            kpis.CustoTotal,
            kpis.Lucro,
            ranking.Select(r => new RankingProduto(r.Nome, (int)r.UnidadesVendidas, r.Receita)).ToList());
    }

    private sealed record DapperKpis(long TotalVendas, decimal ReceitaBruta, decimal CustoTotal, decimal Lucro);
    private sealed record DapperRanking(string Nome, long UnidadesVendidas, decimal Receita);
}
