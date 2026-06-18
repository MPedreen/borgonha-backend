using Borgonha.Api.Authentication;
using Borgonha.Api.Json;
using Borgonha.Infrastructure;
using Borgonha.Service;
using Borgonha.Service.Usuarios;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplicationServices();

builder.Services.AddHttpClient<IUsuarioService, UsuarioService>();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    var namingPolicy = new SnakeCaseNamingPolicy();
    options.JsonSerializerOptions.PropertyNamingPolicy = namingPolicy;
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(namingPolicy));
});

builder.Services.AddTransient<IClaimsTransformation, KeycloakRolesClaimsTransformation>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Keycloak:Authority"];
        options.Audience = builder.Configuration["Keycloak:Audience"];
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters.NameClaimType = "preferred_username";

        // Em Docker, o backend descobre os metadados via hostname interno (keycloak:8080),
        // mas o iss do token usa o hostname público (localhost:8080, via KC_HOSTNAME).
        // MetadataAddress permite separar a URL de discovery da URL de validação do issuer.
        var metadataAddress = builder.Configuration["Keycloak:MetadataAddress"];
        if (!string.IsNullOrEmpty(metadataAddress))
            options.MetadataAddress = metadataAddress;
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    options.AddPolicy("AdminOnly", policy => policy.RequireRole("admin"));
    options.AddPolicy("AtendentePlus", policy => policy.RequireRole("admin", "atendente"));
});

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
