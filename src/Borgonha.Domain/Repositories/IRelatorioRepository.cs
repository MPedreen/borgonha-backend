using Borgonha.Domain.Models;

namespace Borgonha.Domain.Repositories;

public interface IRelatorioRepository
{
    Task<RelatorioDiario> ObterDiarioAsync(DateOnly data, CancellationToken cancellationToken = default);
    Task<RelatorioMensal> ObterMensalAsync(int ano, int mes, CancellationToken cancellationToken = default);
}
