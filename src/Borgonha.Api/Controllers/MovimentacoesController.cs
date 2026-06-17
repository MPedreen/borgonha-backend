using Borgonha.Api.Extensions;
using Borgonha.Service.Relatorios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Borgonha.Api.Controllers;

[ApiController]
[Route("movimentacoes")]
[Authorize(Policy = "AdminOnly")]
public sealed class MovimentacoesController(IRelatorioService relatorioService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ObterPorIngrediente(
        [FromQuery] Guid ingredienteId,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        CancellationToken cancellationToken = default)
    {
        var resultado = await relatorioService.ObterMovimentacoesAsync(ingredienteId, page, size, cancellationToken);
        return resultado.ParaActionResult();
    }
}
