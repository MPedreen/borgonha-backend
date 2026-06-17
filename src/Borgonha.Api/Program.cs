using Borgonha.Api.Authentication;
using Borgonha.Api.Json;
using Borgonha.Infrastructure;
using Borgonha.Service;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplicationServices();

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
