using Borgonha.Domain.Errors;
using Borgonha.Domain.Primitives;
using Microsoft.AspNetCore.Mvc;

namespace Borgonha.Api.Extensions;

public static class ResultExtensions
{
    public static IActionResult ParaActionResult(this Result resultado)
        => resultado.Sucesso ? new NoContentResult() : ParaErroActionResult(resultado.Erro);

    public static IActionResult ParaActionResult<T>(this Result<T> resultado)
        => resultado.Sucesso ? new OkObjectResult(resultado.Valor) : ParaErroActionResult(resultado.Erro);

    private static IActionResult ParaErroActionResult(Error erro)
    {
        var statusCode = erro.Tipo switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.UnprocessableEntity => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status500InternalServerError
        };

        return new ObjectResult(erro) { StatusCode = statusCode };
    }
}
