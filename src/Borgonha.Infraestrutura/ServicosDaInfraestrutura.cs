using Borgonha.Aplicacao.Catalogo;
using Borgonha.Aplicacao.Duvidas;
using Borgonha.Aplicacao.Lojas;
using Borgonha.Infraestrutura.Persistencia;
using Borgonha.Infraestrutura.Repositorios;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Borgonha.Infraestrutura;

public static class ServicosDaInfraestrutura
{
    public static IServiceCollection AdicionarInfraestrutura(this IServiceCollection services, string conexaoBanco)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(conexaoBanco);

        services.AddDbContext<BorgonhaDbContext>(opcoes => opcoes.UseSqlite(conexaoBanco));

        services.AddScoped<ICatalogoRepositorio, CatalogoRepositorio>();
        services.AddScoped<ILojaRepositorio, LojaRepositorio>();
        services.AddScoped<IDuvidaRepositorio, DuvidaRepositorio>();

        services.AddHealthChecks().AddDbContextCheck<BorgonhaDbContext>("banco-de-dados");

        return services;
    }
}
