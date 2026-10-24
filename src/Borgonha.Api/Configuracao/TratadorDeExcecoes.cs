using Microsoft.AspNetCore.Diagnostics;

namespace Borgonha.Api.Configuracao;

/// <summary>
/// Registra erros não tratados e responde com um ProblemDetails (RFC 9457), sem expor detalhes internos.
/// </summary>
internal sealed partial class TratadorDeExcecoes(
    IProblemDetailsService problemDetails,
    ILogger<TratadorDeExcecoes> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        RegistrarErro(logger, exception, httpContext.Request.Path);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails =
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Ocorreu um erro inesperado. Tente novamente em instantes.",
            },
        }).ConfigureAwait(false);
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Erro não tratado ao processar {Caminho}.")]
    private static partial void RegistrarErro(ILogger logger, Exception exception, PathString caminho);
}
