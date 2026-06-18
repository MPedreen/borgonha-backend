namespace Borgonha.Service.Usuarios.Dtos;

public sealed record UsuarioDto(
    string Id,
    string Username,
    string Email,
    string Role);
