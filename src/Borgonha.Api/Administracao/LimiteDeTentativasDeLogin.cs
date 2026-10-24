using System.Threading.RateLimiting;
using Microsoft.Extensions.Options;

namespace Borgonha.Api.Administracao;

/// <summary>
/// Freia tentativas de adivinhar a senha: um limite por endereço IP (a cada 15 minutos) e um
/// limite geral (a cada hora), que vale mesmo para quem troca de endereço a cada tentativa.
/// </summary>
internal sealed class LimiteDeTentativasDeLogin : IDisposable
{
    private readonly PartitionedRateLimiter<string> _limitador;

    public LimiteDeTentativasDeLogin(IOptions<AdministradorOpcoes> opcoes)
    {
        var configuracao = opcoes.Value;

        _limitador = PartitionedRateLimiter.CreateChained(
            PartitionedRateLimiter.Create<string, string>(endereco => RateLimitPartition.GetFixedWindowLimiter(
                endereco,
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = configuracao.TentativasPorEndereco,
                    Window = TimeSpan.FromMinutes(15),
                    QueueLimit = 0,
                })),
            PartitionedRateLimiter.Create<string, string>(_ => RateLimitPartition.GetFixedWindowLimiter(
                "todas",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = configuracao.TentativasPorHora,
                    Window = TimeSpan.FromHours(1),
                    QueueLimit = 0,
                })));
    }

    /// <returns><see langword="false"/> se o limite de tentativas foi atingido.</returns>
    public bool PodeTentar(string endereco)
    {
        using var permissao = _limitador.AttemptAcquire(endereco);

        return permissao.IsAcquired;
    }

    public void Dispose() => _limitador.Dispose();
}
