using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace Borgonha.Api.Configuracao;

/// <summary>
/// Limite de consultas de entrega por endereço IP. Cada consulta pode gastar crédito do serviço de mapas,
/// então uma pessoa (ou um robô) não pode consumir sozinha o limite do dia de todos os clientes.
/// </summary>
internal static class LimiteDeConsultasDeEntrega
{
    public const string Politica = "entrega";
    public const string ChaveDaConfiguracao = "Entrega:ConsultasPorMinutoPorEndereco";
    public const int PadraoPorMinuto = 30;

    public static IServiceCollection AdicionarLimiteDeConsultasDeEntrega(this IServiceCollection services, IConfiguration configuracao)
    {
        var porMinuto = Math.Max(1, configuracao.GetValue(ChaveDaConfiguracao, PadraoPorMinuto));

        services.AddRateLimiter(opcoes =>
        {
            opcoes.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            opcoes.AddPolicy(Politica, contexto => RateLimitPartition.GetSlidingWindowLimiter(
                contexto.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
                _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = porMinuto,
                    Window = TimeSpan.FromMinutes(1),
                    SegmentsPerWindow = 6,
                    QueueLimit = 0,
                }));

            opcoes.OnRejected = async (contexto, cancellationToken) =>
            {
                contexto.HttpContext.Response.Headers.RetryAfter = "30";
                var problemas = contexto.HttpContext.RequestServices.GetRequiredService<IProblemDetailsService>();

                await problemas.TryWriteAsync(new ProblemDetailsContext
                {
                    HttpContext = contexto.HttpContext,
                    ProblemDetails =
                    {
                        Status = StatusCodes.Status429TooManyRequests,
                        Title = "Muitas consultas seguidas. Espere alguns instantes e tente de novo.",
                    },
                }).ConfigureAwait(false);
            };
        });

        return services;
    }
}
