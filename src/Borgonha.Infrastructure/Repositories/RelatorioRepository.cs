using Borgonha.Domain.Enums;
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

        // Dapper não suporta DateOnly nativamente — converte para DateTime (Kind=Unspecified → timestamp without time zone no Npgsql)
        var dataParam = new DateTime(data.Year, data.Month, data.Day);
        using var conn = connectionFactory.Criar();
        var row = await conn.QuerySingleAsync<DapperKpis>(sql, new { Data = dataParam });
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

    public async Task<PaginaMovimentacoes> ObterMovimentacoesAsync(Guid ingredienteId, int pagina, int tamanho, CancellationToken cancellationToken = default)
    {
        const string sqlTotal = """
            SELECT COUNT(*) FROM movimentacoes_estoque WHERE ingrediente_id = @IngredienteId
            """;

        const string sqlItens = """
            SELECT
                id          AS Id,
                tipo        AS Tipo,
                quantidade  AS Quantidade,
                data_hora   AS DataHora,
                venda_id    AS VendaId,
                observacao  AS Observacao
            FROM movimentacoes_estoque
            WHERE ingrediente_id = @IngredienteId
            ORDER BY data_hora DESC
            LIMIT @Tamanho OFFSET @Offset
            """;

        var offset = (pagina - 1) * tamanho;
        var param = new { IngredienteId = ingredienteId, Tamanho = tamanho, Offset = offset };

        using var conn = connectionFactory.Criar();
        var total = await conn.ExecuteScalarAsync<int>(sqlTotal, new { IngredienteId = ingredienteId });
        var rows = (await conn.QueryAsync<DapperMovimentacao>(sqlItens, param)).ToList();

        var itens = rows.Select(r => new MovimentacaoItem(
            r.Id,
            Enum.Parse<TipoMovimentacao>(r.Tipo, ignoreCase: true),
            r.Quantidade,
            r.DataHora,
            r.VendaId,
            r.Observacao)).ToList();

        return new PaginaMovimentacoes(total, pagina, tamanho, itens);
    }

    private sealed class DapperKpis
    {
        public long TotalVendas { get; set; }
        public decimal ReceitaBruta { get; set; }
        public decimal CustoTotal { get; set; }
        public decimal Lucro { get; set; }
    }

    private sealed class DapperRanking
    {
        public string Nome { get; set; } = "";
        public long UnidadesVendidas { get; set; }
        public decimal Receita { get; set; }
    }

    private sealed class DapperMovimentacao
    {
        public Guid Id { get; set; }
        public string Tipo { get; set; } = "";
        public decimal Quantidade { get; set; }
        public DateTime DataHora { get; set; }
        public Guid? VendaId { get; set; }
        public string? Observacao { get; set; }
    }
}
