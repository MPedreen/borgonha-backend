using Borgonha.Api.Extensions;
using Borgonha.Service.Relatorios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Borgonha.Api.Controllers;

[ApiController]
[Route("relatorios")]
[Authorize(Policy = "AdminOnly")]
public sealed class RelatoriosController(IRelatorioService relatorioService) : ControllerBase
{
    [HttpGet("diario")]
    public async Task<IActionResult> ObterDiario([FromQuery] DateOnly data, CancellationToken cancellationToken)
    {
        var resultado = await relatorioService.ObterDiarioAsync(data, cancellationToken);
        return resultado.ParaActionResult();
    }

    [HttpGet("mensal")]
    public async Task<IActionResult> ObterMensal([FromQuery] int ano, [FromQuery] int mes, CancellationToken cancellationToken)
    {
        var resultado = await relatorioService.ObterMensalAsync(ano, mes, cancellationToken);
        return resultado.ParaActionResult();
    }
}
