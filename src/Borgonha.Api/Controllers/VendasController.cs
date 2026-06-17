using Borgonha.Api.Extensions;
using Borgonha.Service.Pdv;
using Borgonha.Service.Pdv.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Borgonha.Api.Controllers;

[ApiController]
[Route("vendas")]
[Authorize(Policy = "AtendentePlus")]
public sealed class VendasController(IVendaService vendaService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Registrar(RegistrarVendaRequest request, CancellationToken cancellationToken)
    {
        var criadoPor = User.Identity?.Name ?? string.Empty;

        var resultado = await vendaService.RegistrarAsync(criadoPor, request, cancellationToken);
        return resultado.Sucesso
            ? Created($"/vendas/{resultado.Valor.Id}", resultado.Valor)
            : resultado.ParaActionResult();
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await vendaService.ObterPorIdAsync(id, cancellationToken);
        return resultado.ParaActionResult();
    }
}
