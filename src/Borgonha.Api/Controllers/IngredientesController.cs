using Borgonha.Api.Extensions;
using Borgonha.Service.Estoque;
using Borgonha.Service.Estoque.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Borgonha.Api.Controllers;

[ApiController]
[Route("ingredientes")]
[Authorize(Policy = "AdminOnly")]
public sealed class IngredientesController(IIngredienteService ingredienteService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ObterTodos(CancellationToken cancellationToken)
    {
        var resultado = await ingredienteService.ObterTodosAsync(cancellationToken);
        return resultado.ParaActionResult();
    }

    [HttpGet("alertas")]
    public async Task<IActionResult> ObterEmAlerta(CancellationToken cancellationToken)
    {
        var resultado = await ingredienteService.ObterEmAlertaAsync(cancellationToken);
        return resultado.ParaActionResult();
    }

    [HttpPost]
    public async Task<IActionResult> Criar(CriarIngredienteRequest request, CancellationToken cancellationToken)
    {
        var resultado = await ingredienteService.CriarAsync(request, cancellationToken);
        return resultado.Sucesso
            ? Created($"/ingredientes/{resultado.Valor.Id}", resultado.Valor)
            : resultado.ParaActionResult();
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, AtualizarIngredienteRequest request, CancellationToken cancellationToken)
    {
        var resultado = await ingredienteService.AtualizarAsync(id, request, cancellationToken);
        return resultado.ParaActionResult();
    }

    [HttpPatch("{id:guid}/entrada")]
    public async Task<IActionResult> RegistrarEntrada(Guid id, RegistrarEntradaRequest request, CancellationToken cancellationToken)
    {
        var resultado = await ingredienteService.RegistrarEntradaAsync(id, request, cancellationToken);
        return resultado.ParaActionResult();
    }
}
