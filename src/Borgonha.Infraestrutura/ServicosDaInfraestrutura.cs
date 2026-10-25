using Borgonha.Aplicacao.Administracao;
using Borgonha.Aplicacao.Catalogo;
using Borgonha.Aplicacao.Duvidas;
using Borgonha.Aplicacao.Lojas;
using Borgonha.Infraestrutura.Imagens;
using Borgonha.Infraestrutura.Persistencia;
using Borgonha.Infraestrutura.Repositorios;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Borgonha.Infraestrutura;

public static class ServicosDaInfraestrutura
{
    /// <param name="conexaoBanco">Connection string do PostgreSQL.</param>
    /// <param name="pastaDasImagens">Pasta absoluta onde ficam as fotos enviadas pela área do administrador.</param>
    public static IServiceCollection AdicionarInfraestrutura(this IServiceCollection services, string conexaoBanco, string pastaDasImagens)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(conexaoBanco);
        ArgumentException.ThrowIfNullOrWhiteSpace(pastaDasImagens);

        // Tabelas e colunas em snake_case (produtos, categoria_id): no PostgreSQL, nomes com maiúsculas
        // exigem aspas em toda consulta SQL escrita à mão.
        services.AddDbContext<BorgonhaDbContext>(opcoes => opcoes
            .UseNpgsql(conexaoBanco, postgres => postgres.EnableRetryOnFailure())
            .UseSnakeCaseNamingConvention()
            .UseSeeding(DadosIniciais.Semear)
            .UseAsyncSeeding(DadosIniciais.SemearAsync));

        services.AddScoped<ICatalogoRepositorio, CatalogoRepositorio>();
        services.AddScoped<ILojaRepositorio, LojaRepositorio>();
        services.AddScoped<IDuvidaRepositorio, DuvidaRepositorio>();

        services.AddSingleton(new OpcoesDasImagens(pastaDasImagens));
        services.AddSingleton<IArmazenamentoDeImagens, ArmazenamentoDeImagensEmDisco>();

        services.AddHealthChecks().AddDbContextCheck<BorgonhaDbContext>("banco-de-dados");

        return services;
    }

    /// <summary>
    /// Cria ou atualiza o banco (migrations) e cadastra os dados iniciais quando ele está vazio.
    /// </summary>
    public static async Task AplicarMigracoesAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        var escopo = services.CreateAsyncScope();
        await using (escopo.ConfigureAwait(false))
        {
            var contexto = escopo.ServiceProvider.GetRequiredService<BorgonhaDbContext>();
            await contexto.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
