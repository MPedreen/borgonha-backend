using Npgsql;
using Testcontainers.PostgreSql;

namespace Borgonha.Api.Tests;

/// <summary>
/// Um único PostgreSQL, em contêiner, para toda a execução dos testes: subir um por classe de teste seria lento.
/// Cada fábrica da API usa um banco só dela dentro desse servidor, criado pelas migrations quando a API inicia.
/// O contêiner é removido ao fim da execução pelo Ryuk, o limpador do Testcontainers.
/// </summary>
internal static class ServidorDePostgres
{
    // Mesma versão do docker-compose.yml, para os testes rodarem no banco que a loja usa.
    private const string Imagem = "postgres:18-alpine";

    private static readonly Lazy<Task<string>> ConexaoDoServidor = new(IniciarAsync);

    /// <summary>Connection string de um banco novo e vazio (ainda por criar) no servidor dos testes.</summary>
    public static async Task<string> NovaConexaoAsync()
    {
        var servidor = await ConexaoDoServidor.Value.ConfigureAwait(false);
        return new NpgsqlConnectionStringBuilder(servidor) { Database = $"borgonha_testes_{Guid.NewGuid():N}" }.ConnectionString;
    }

    private static async Task<string> IniciarAsync()
    {
        var conteiner = new PostgreSqlBuilder(Imagem).Build();
        await conteiner.StartAsync().ConfigureAwait(false);
        return conteiner.GetConnectionString();
    }
}
