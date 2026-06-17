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
}
