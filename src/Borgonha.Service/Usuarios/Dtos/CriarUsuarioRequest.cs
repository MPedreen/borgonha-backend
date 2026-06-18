namespace Borgonha.Service.Usuarios.Dtos;

public sealed record CriarUsuarioRequest(
    string Nome,
    string Sobrenome,
    string Email,
    string Username,
    string Senha,
    string Role);
