using Borgonha.Aplicacao.Administracao;
using Borgonha.Dominio.Comum;
using Microsoft.AspNetCore.Diagnostics;

namespace Borgonha.Api.Configuracao;

/// <summary>
/// Responde erros com um ProblemDetails (RFC 9457). Regras de negócio violadas viram 400 com a
/// mensagem para quem está usando; erros inesperados são registrados e não expõem detalhes internos.
/// </summary>
internal sealed partial class TratadorDeExcecoes(
    IProblemDetailsService problemDetails,
    ILogger<TratadorDeExcecoes> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, titulo) = exception switch
        {
            DominioException or ImagemInvalidaException => (StatusCodes.Status400BadRequest, exception.Message),
            BadHttpRequestException { StatusCode: StatusCodes.Status413PayloadTooLarge } =>
                (StatusCodes.Status413PayloadTooLarge, "O arquivo enviado é grande demais."),
            BadHttpRequestException requisicaoInvalida => (requisicaoInvalida.StatusCode, "Requisição inválida."),
            _ => (StatusCodes.Status500InternalServerError, "Ocorreu um erro inesperado. Tente novamente em instantes."),
        };

        if (status == StatusCodes.Status500InternalServerError)
        {
            RegistrarErro(logger, exception, httpContext.Request.Path);
        }

        httpContext.Response.StatusCode = status;

        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails =
            {
                Status = status,
                Title = titulo,
            },
        }).ConfigureAwait(false);
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Erro não tratado ao processar {Caminho}.")]
    private static partial void RegistrarErro(ILogger logger, Exception exception, PathString caminho);
}
