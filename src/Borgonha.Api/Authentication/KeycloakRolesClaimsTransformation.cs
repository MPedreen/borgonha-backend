using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;

namespace Borgonha.Api.Authentication;

public sealed class KeycloakRolesClaimsTransformation : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var identity = principal.Identities.FirstOrDefault(i => i.IsAuthenticated);
        var realmAccess = identity?.FindFirst("realm_access")?.Value;

        if (identity is null || string.IsNullOrWhiteSpace(realmAccess))
            return Task.FromResult(principal);

        using var documento = JsonDocument.Parse(realmAccess);
        if (!documento.RootElement.TryGetProperty("roles", out var roles))
            return Task.FromResult(principal);

        foreach (var role in roles.EnumerateArray())
        {
            var nomeRole = role.GetString();
            if (!string.IsNullOrWhiteSpace(nomeRole) && !identity.HasClaim(ClaimTypes.Role, nomeRole))
                identity.AddClaim(new Claim(ClaimTypes.Role, nomeRole));
        }

        return Task.FromResult(principal);
    }
}
