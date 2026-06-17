using Borgonha.Domain.Repositories;
using Borgonha.Infrastructure.Dapper;
using Borgonha.Infrastructure.Persistence;
using Borgonha.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Borgonha.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Lê a connection string via factory delegate para que o override do WebApplicationFactory
        // (ConfigureAppConfiguration) seja aplicado antes da resolução, não no momento do registro.
        services.AddDbContext<BorgonhaDbContext>((sp, options) =>
        {
            var connStr = sp.GetRequiredService<IConfiguration>().GetConnectionString("Default")
                ?? throw new InvalidOperationException("Connection string 'Default' não configurada.");
            options.UseNpgsql(connStr).UseSnakeCaseNamingConvention();
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<BorgonhaDbContext>());

        services.AddScoped<IProdutoRepository, ProdutoRepository>();
        services.AddScoped<IIngredienteRepository, IngredienteRepository>();
        services.AddScoped<IVendaRepository, VendaRepository>();

        services.AddSingleton<ConnectionFactory>();
        services.AddScoped<IRelatorioRepository, RelatorioRepository>();

        return services;
    }
}
