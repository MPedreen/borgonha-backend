using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;

namespace Borgonha.Infrastructure.Dapper;

internal sealed class ConnectionFactory(IConfiguration configuration)
{
    public IDbConnection Criar() =>
        new NpgsqlConnection(configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' não configurada."));
}
