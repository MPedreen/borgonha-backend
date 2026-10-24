using Borgonha.Api.Administracao;
using Borgonha.Api.Configuracao;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;

namespace Borgonha.Api.Endpoints;

/// <summary>
/// Entrar e sair da área do administrador.
/// </summary>
internal static partial class SessaoAdminEndpoints
{
    public static RouteGroupBuilder MapearSessaoAdmin(this RouteGroupBuilder admin)
    {
        var sessao = admin.MapGroup("/sessao");

        sessao.MapPost("/", EntrarAsync)
            .AllowAnonymous()
            .WithName("EntrarNaAreaDoAdministrador")
            .WithSummary("Confere usuário e senha e abre a sessão (cookie HttpOnly).");

        sessao.MapGet("/", () => TypedResults.NoContent())
            .RequireAuthorization(AutenticacaoDoAdministrador.Politica)
            .WithName("ConferirSessaoDoAdministrador")
            .WithSummary("204 se a sessão está aberta; 401 se não está.");

        // Conversão explícita: com só o HttpContext, o método seria tratado como RequestDelegate (ASP0016).
        sessao.MapDelete("/", (Func<HttpContext, Task<NoContent>>)SairAsync)
            .AllowAnonymous()
            .WithName("SairDaAreaDoAdministrador")
            .WithSummary("Encerra a sessão.");

        return admin;
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> EntrarAsync(
        CredenciaisDto credenciais,
        HttpContext contexto,
        VerificadorDeCredenciais verificador,
        LimiteDeTentativasDeLogin limite,
        IOptionsMonitor<AdministradorOpcoes> opcoes,
        ILoggerFactory logs)
    {
        if (!opcoes.CurrentValue.Configurado)
        {
            return TypedResults.Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "A área do administrador ainda não foi configurada.");
        }

        var endereco = contexto.Connection.RemoteIpAddress?.ToString() ?? "desconhecido";
        var logger = logs.CreateLogger(typeof(SessaoAdminEndpoints));

        if (!limite.PodeTentar(endereco))
        {
            RegistrarBloqueio(logger, endereco);
            return TypedResults.Problem(
                statusCode: StatusCodes.Status429TooManyRequests,
                title: "Muitas tentativas de entrar. Espere alguns minutos e tente de novo.");
        }

        if (!verificador.Verificar(credenciais.Usuario, credenciais.Senha))
        {
            RegistrarRecusa(logger, endereco);
            return TypedResults.Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Usuário ou senha incorretos.");
        }

        await contexto.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, AutenticacaoDoAdministrador.CriarSessao(verificador))
            .ConfigureAwait(false);
        RegistrarEntrada(logger, endereco);

        return TypedResults.NoContent();
    }

    private static async Task<NoContent> SairAsync(HttpContext contexto)
    {
        await contexto.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).ConfigureAwait(false);
        return TypedResults.NoContent();
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Login na área do administrador recusado (endereço {Endereco}).")]
    private static partial void RegistrarRecusa(ILogger logger, string endereco);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Limite de tentativas de login atingido (endereço {Endereco}).")]
    private static partial void RegistrarBloqueio(ILogger logger, string endereco);

    [LoggerMessage(Level = LogLevel.Information, Message = "Administrador entrou (endereço {Endereco}).")]
    private static partial void RegistrarEntrada(ILogger logger, string endereco);
}

internal sealed record CredenciaisDto(string? Usuario, string? Senha);
