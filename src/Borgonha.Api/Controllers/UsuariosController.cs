using Borgonha.Api.Extensions;
using Borgonha.Service.Usuarios;
using Borgonha.Service.Usuarios.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Borgonha.Api.Controllers;

[ApiController]
[Route("usuarios")]
public sealed class UsuariosController(IUsuarioService usuarioService) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Criar(CriarUsuarioRequest request, CancellationToken cancellationToken)
    {
        var resultado = await usuarioService.CriarAsync(request, cancellationToken);
        return resultado.Sucesso
            ? Created($"/usuarios/{resultado.Valor.Id}", resultado.Valor)
            : resultado.ParaActionResult();
    }
}
