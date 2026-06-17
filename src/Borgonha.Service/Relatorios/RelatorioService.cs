using Borgonha.Domain.Models;
using Borgonha.Domain.Primitives;
using Borgonha.Domain.Repositories;

namespace Borgonha.Service.Relatorios;

internal sealed class RelatorioService(IRelatorioRepository relatorioRepository) : IRelatorioService
{
    public async Task<Result<RelatorioDiario>> ObterDiarioAsync(DateOnly data, CancellationToken cancellationToken = default)
    {
        var relatorio = await relatorioRepository.ObterDiarioAsync(data, cancellationToken);
        return Result.Ok(relatorio);
    }

    public async Task<Result<RelatorioMensal>> ObterMensalAsync(int ano, int mes, CancellationToken cancellationToken = default)
    {
        var relatorio = await relatorioRepository.ObterMensalAsync(ano, mes, cancellationToken);
        return Result.Ok(relatorio);
    }

    public async Task<Result<PaginaMovimentacoes>> ObterMovimentacoesAsync(Guid ingredienteId, int pagina, int tamanho, CancellationToken cancellationToken = default)
    {
        if (pagina < 1) pagina = 1;
        if (tamanho < 1 || tamanho > 100) tamanho = 20;

        var pagina_ = await relatorioRepository.ObterMovimentacoesAsync(ingredienteId, pagina, tamanho, cancellationToken);
        return Result.Ok(pagina_);
    }
}
