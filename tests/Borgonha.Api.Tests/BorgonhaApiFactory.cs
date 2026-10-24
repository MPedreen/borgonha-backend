using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;

namespace Borgonha.Api.Tests;

/// <summary>
/// Sobe a API inteira em memória, com um banco SQLite em memória criado pelas migrations e pelos dados iniciais.
/// </summary>
public sealed class BorgonhaApiFactory : WebApplicationFactory<Program>
{
    public const string OrigemDoSite = "https://www.borgonha.test";

    private readonly string _conexao = $"Data Source=borgonha-testes-{Guid.NewGuid():N};Mode=Memory;Cache=Shared";
    private readonly SqliteConnection _conexaoMantida;

    public BorgonhaApiFactory()
    {
        // O banco em memória existe enquanto houver uma conexão aberta com ele.
        _conexaoMantida = new SqliteConnection(_conexao);
        _conexaoMantida.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testes");
        builder.UseSetting("ConnectionStrings:Borgonha", _conexao);
        builder.UseSetting("Cors:OrigensPermitidas:0", OrigemDoSite);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _conexaoMantida.Dispose();
        }
    }
}
