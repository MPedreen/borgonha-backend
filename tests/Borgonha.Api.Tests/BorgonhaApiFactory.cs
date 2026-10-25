using Borgonha.Api.Administracao;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Borgonha.Api.Tests;

/// <summary>
/// Sobe a API inteira em memória, com um banco PostgreSQL só dela (criado pelas migrations e pelos dados iniciais),
/// pastas temporárias para fotos e chaves e uma credencial de administrador só para os testes.
/// </summary>
public class BorgonhaApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string OrigemDoSite = "https://www.borgonha.test";
    public const string UsuarioAdmin = "dona-da-loja";
    public const string SenhaAdmin = "senha-so-para-testes";

    private static readonly Lazy<string> HashDaSenhaAdmin = new(() => HashDeSenha.Gerar(SenhaAdmin));

    private readonly string _pastaTemporaria = Path.Combine(Path.GetTempPath(), $"borgonha-testes-{Guid.NewGuid():N}");
    private string _conexao = string.Empty;

    public string PastaDasImagens => Path.Combine(_pastaTemporaria, "imagens");

    /// <summary>Tentativas de login por endereço; alto, para os testes não esbarrarem no limite.</summary>
    protected virtual int TentativasDeLogin => 1_000;

    /// <summary>Cliente HTTPS (o cookie da sessão só trafega por HTTPS fora do desenvolvimento).</summary>
    public HttpClient CriarClienteHttps() =>
        CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

    public async ValueTask InitializeAsync() => _conexao = await ServidorDePostgres.NovaConexaoAsync().ConfigureAwait(false);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testes");
        builder.UseSetting("ConnectionStrings:Borgonha", _conexao);
        builder.UseSetting("Cors:OrigensPermitidas:0", OrigemDoSite);
        builder.UseSetting("Armazenamento:PastaDasImagens", PastaDasImagens);
        builder.UseSetting("Armazenamento:PastaDasChaves", Path.Combine(_pastaTemporaria, "chaves"));
        builder.UseSetting("Administrador:Usuario", UsuarioAdmin);
        builder.UseSetting("Administrador:SenhaHash", HashDaSenhaAdmin.Value);
        builder.UseSetting("Administrador:TentativasPorEndereco", TentativasDeLogin.ToString(System.Globalization.CultureInfo.InvariantCulture));
        builder.UseSetting("Administrador:TentativasPorHora", "1000");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing && Directory.Exists(_pastaTemporaria))
        {
            Directory.Delete(_pastaTemporaria, recursive: true);
        }
    }
}
