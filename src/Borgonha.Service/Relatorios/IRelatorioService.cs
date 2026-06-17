using Borgonha.Domain.Models;
using Borgonha.Domain.Primitives;

namespace Borgonha.Service.Relatorios;

public interface IRelatorioService
{
    Task<Result<RelatorioDiario>> ObterDiarioAsync(DateOnly data, CancellationToken cancellationToken = default);
    Task<Result<RelatorioMensal>> ObterMensalAsync(int ano, int mes, CancellationToken cancellationToken = default);
}
