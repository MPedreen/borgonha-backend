using Borgonha.Api.Extensions;
using Borgonha.Service.Produtos;
using Borgonha.Service.Produtos.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Borgonha.Api.Controllers;

[ApiController]
[Route("produtos")]
public sealed class ProdutosController(IProdutoService produtoService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "AtendentePlus")]
    public async Task<IActionResult> ObterAtivos(CancellationToken cancellationToken)
    {
        var resultado = await produtoService.ObterAtivosAsync(cancellationToken);
        return resultado.ParaActionResult();
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Criar(CriarProdutoRequest request, CancellationToken cancellationToken)
    {
        var resultado = await produtoService.CriarAsync(request, cancellationToken);
        return resultado.Sucesso
            ? Created($"/produtos/{resultado.Valor.Id}", resultado.Valor)
            : resultado.ParaActionResult();
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Atualizar(Guid id, AtualizarProdutoRequest request, CancellationToken cancellationToken)
    {
        var resultado = await produtoService.AtualizarAsync(id, request, cancellationToken);
        return resultado.ParaActionResult();
    }
}
