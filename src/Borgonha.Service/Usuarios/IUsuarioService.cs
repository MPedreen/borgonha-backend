using Borgonha.Domain.Primitives;
using Borgonha.Service.Usuarios.Dtos;

namespace Borgonha.Service.Usuarios;

public interface IUsuarioService
{
    Task<Result<UsuarioDto>> CriarAsync(CriarUsuarioRequest request, CancellationToken cancellationToken = default);
}
