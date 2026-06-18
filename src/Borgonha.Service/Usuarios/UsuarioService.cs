using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Borgonha.Domain.Errors;
using Borgonha.Domain.Primitives;
using Borgonha.Service.Usuarios.Dtos;
using Microsoft.Extensions.Configuration;

namespace Borgonha.Service.Usuarios;

public sealed class UsuarioService(HttpClient httpClient, IConfiguration configuration) : IUsuarioService
{
    private static readonly string[] RolesPermitidas = ["admin", "atendente"];

    public async Task<Result<UsuarioDto>> CriarAsync(CriarUsuarioRequest request, CancellationToken cancellationToken = default)
    {
        if (!RolesPermitidas.Contains(request.Role))
            return Result.Falha<UsuarioDto>(UsuariosErrors.Usuario.RoleInvalida);

        var adminToken = await ObterTokenAdminAsync(cancellationToken);
        if (adminToken is null)
            return Result.Falha<UsuarioDto>(UsuariosErrors.Usuario.ErroCriar);

        var adminUrl = configuration["Keycloak:AdminUrl"]!;

        var keycloakUser = new
        {
            username = request.Username,
            email = request.Email,
            firstName = request.Nome,
            lastName = request.Sobrenome,
            enabled = true,
            emailVerified = true,
            credentials = new[] { new { type = "password", value = request.Senha, temporary = false } }
        };

        var criarReq = new HttpRequestMessage(HttpMethod.Post, $"{adminUrl}/admin/realms/borgonha/users");
        criarReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        criarReq.Content = JsonContent.Create(keycloakUser);

        var criarRes = await httpClient.SendAsync(criarReq, cancellationToken);

        if (criarRes.StatusCode == HttpStatusCode.Conflict)
            return Result.Falha<UsuarioDto>(UsuariosErrors.Usuario.JaExiste);

        if (!criarRes.IsSuccessStatusCode)
            return Result.Falha<UsuarioDto>(UsuariosErrors.Usuario.ErroCriar);

        var location = criarRes.Headers.Location?.ToString() ?? string.Empty;
        var userId = location.Split('/').Last();

        // Busca a representação completa da role (Keycloak exige id + name)
        var roleReq = new HttpRequestMessage(HttpMethod.Get, $"{adminUrl}/admin/realms/borgonha/roles/{request.Role}");
        roleReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var roleRes = await httpClient.SendAsync(roleReq, cancellationToken);
        if (!roleRes.IsSuccessStatusCode)
            return Result.Falha<UsuarioDto>(UsuariosErrors.Usuario.ErroCriar);

        var role = await roleRes.Content.ReadFromJsonAsync<KeycloakRoleDto>(cancellationToken: cancellationToken);
        if (role is null)
            return Result.Falha<UsuarioDto>(UsuariosErrors.Usuario.ErroCriar);

        var atribuirReq = new HttpRequestMessage(HttpMethod.Post,
            $"{adminUrl}/admin/realms/borgonha/users/{userId}/role-mappings/realm");
        atribuirReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        atribuirReq.Content = JsonContent.Create(new[] { role });

        await httpClient.SendAsync(atribuirReq, cancellationToken);

        return Result.Ok(new UsuarioDto(userId, request.Username, request.Email, request.Role));
    }

    private async Task<string?> ObterTokenAdminAsync(CancellationToken cancellationToken)
    {
        var adminUrl = configuration["Keycloak:AdminUrl"]!;
        var username = configuration["Keycloak:AdminUsername"]!;
        var password = configuration["Keycloak:AdminPassword"]!;

        var body = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = "admin-cli",
            ["username"] = username,
            ["password"] = password,
        });

        var res = await httpClient.PostAsync(
            $"{adminUrl}/realms/master/protocol/openid-connect/token", body, cancellationToken);

        if (!res.IsSuccessStatusCode) return null;

        var data = await res.Content.ReadFromJsonAsync<KeycloakTokenDto>(cancellationToken: cancellationToken);
        return data?.AccessToken;
    }

    private sealed record KeycloakTokenDto([property: JsonPropertyName("access_token")] string AccessToken);

    private sealed record KeycloakRoleDto(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("name")] string Name);
}
