using Borgonha.Service.Estoque;
using Borgonha.Service.Pdv;
using Borgonha.Service.Produtos;
using Borgonha.Service.Relatorios;
using Microsoft.Extensions.DependencyInjection;

namespace Borgonha.Service;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IProdutoService, ProdutoService>();
        services.AddScoped<IIngredienteService, IngredienteService>();
        services.AddScoped<IVendaService, VendaService>();
        services.AddScoped<IRelatorioService, RelatorioService>();

        // UsuarioService é registrado via AddHttpClient<> em Program.cs (requer HttpClient injetado)

        return services;
    }
}
