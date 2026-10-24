using Borgonha.Aplicacao.Catalogo;
using Borgonha.Aplicacao.Duvidas;
using Borgonha.Aplicacao.Lojas;
using Microsoft.Extensions.DependencyInjection;

namespace Borgonha.Aplicacao;

public static class ServicosDaAplicacao
{
    public static IServiceCollection AdicionarAplicacao(this IServiceCollection services)
    {
        services.AddScoped<ObterCatalogo>();
        services.AddScoped<ObterProduto>();
        services.AddScoped<ObterLoja>();
        services.AddScoped<ListarDuvidas>();

        return services;
    }
}
