using System.Security.Claims;
using Borgonha.Api.Administracao;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;

namespace Borgonha.Api.Configuracao;

/// <summary>
/// Sessão da área do administrador num cookie protegido: HttpOnly (fora do alcance de scripts),
/// SameSite=Strict (não vai em requisições vindas de outros sites) e, em produção, só por HTTPS.
/// </summary>
internal static class AutenticacaoDoAdministrador
{
    public const string Politica = "Administrador";

    /// <summary>
    /// Cabeçalho exigido em toda chamada da área do administrador. Outros sites não conseguem
    /// enviá-lo (o CORS só libera o site da Borgonha), o que barra ataques de CSRF.
    /// </summary>
    public const string Cabecalho = "X-Borgonha-Admin";

    private const string Papel = "administrador";
    private const string ClaimVersaoDaSenha = "versao-da-senha";

    public static IServiceCollection AdicionarAutenticacaoDoAdministrador(
        this IServiceCollection services,
        IConfiguration configuracao,
        IHostEnvironment ambiente,
        string pastaDasChaves)
    {
        services.AddOptions<AdministradorOpcoes>()
            .Bind(configuracao.GetSection(AdministradorOpcoes.Secao))
            .Validate(opcoes => opcoes.EhValido(), "Configuração inválida em Administrador. Gere o hash da senha com: dotnet run --project src/Borgonha.Api -- gerar-hash-senha")
            .ValidateOnStart();

        services.AddSingleton<VerificadorDeCredenciais>();
        services.AddSingleton<LimiteDeTentativasDeLogin>();

        // Chaves que assinam e cifram o cookie, guardadas fora do repositório para a sessão sobreviver a reinícios.
        services.AddDataProtection()
            .SetApplicationName("borgonha-api")
            .PersistKeysToFileSystem(new DirectoryInfo(pastaDasChaves));

        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(opcoes =>
            {
                // Fora do desenvolvimento, o prefixo __Host- obriga HTTPS, o domínio do próprio site e o caminho "/".
                opcoes.Cookie.Name = ambiente.IsDevelopment() ? "borgonha-admin" : "__Host-borgonha-admin";
                opcoes.Cookie.HttpOnly = true;
                opcoes.Cookie.SameSite = SameSiteMode.Strict;
                opcoes.Cookie.SecurePolicy = ambiente.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
                opcoes.Cookie.Path = "/";
                opcoes.SlidingExpiration = true;
                opcoes.Events = new CookieAuthenticationEvents
                {
                    // É uma API: sem redirecionar para páginas de login, só o status.
                    OnRedirectToLogin = contexto => ResponderStatus(contexto, StatusCodes.Status401Unauthorized),
                    OnRedirectToAccessDenied = contexto => ResponderStatus(contexto, StatusCodes.Status403Forbidden),
                    OnValidatePrincipal = ValidarVersaoDaSenhaAsync,
                };
            });

        services.AddOptions<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme)
            .Configure<IOptions<AdministradorOpcoes>>((cookie, administrador) => cookie.ExpireTimeSpan = administrador.Value.DuracaoDaSessao);

        services.AddAuthorizationBuilder()
            .AddPolicy(Politica, politica => politica.RequireRole(Papel));

        return services;
    }

    public static ClaimsPrincipal CriarSessao(VerificadorDeCredenciais verificador) =>
        new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Name, "Administrador"),
                new Claim(ClaimTypes.Role, Papel),
                new Claim(ClaimVersaoDaSenha, verificador.VersaoDaSenha()),
            ],
            CookieAuthenticationDefaults.AuthenticationScheme));

    private static Task ResponderStatus(RedirectContext<CookieAuthenticationOptions> contexto, int status)
    {
        contexto.Response.StatusCode = status;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Se a senha foi trocada depois do login, a sessão deixa de valer.
    /// </summary>
    private static async Task ValidarVersaoDaSenhaAsync(CookieValidatePrincipalContext contexto)
    {
        var verificador = contexto.HttpContext.RequestServices.GetRequiredService<VerificadorDeCredenciais>();

        if (contexto.Principal?.FindFirst(ClaimVersaoDaSenha)?.Value != verificador.VersaoDaSenha())
        {
            contexto.RejectPrincipal();
            await contexto.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).ConfigureAwait(false);
        }
    }
}
