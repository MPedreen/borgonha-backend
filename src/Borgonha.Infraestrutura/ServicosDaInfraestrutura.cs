using Borgonha.Aplicacao.Administracao;
using Borgonha.Aplicacao.Catalogo;
using Borgonha.Aplicacao.Duvidas;
using Borgonha.Aplicacao.Lojas;
using Borgonha.Infraestrutura.Imagens;
using Borgonha.Infraestrutura.Persistencia;
using Borgonha.Infraestrutura.Repositorios;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Borgonha.Infraestrutura;

public static class ServicosDaInfraestrutura
{
    /// <param name="pastaDasImagens">Pasta absoluta onde ficam as fotos enviadas pela área do administrador.</param>
    public static IServiceCollection AdicionarInfraestrutura(this IServiceCollection services, string conexaoBanco, string pastaDasImagens)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(conexaoBanco);
        ArgumentException.ThrowIfNullOrWhiteSpace(pastaDasImagens);

        services.AddDbContext<BorgonhaDbContext>(opcoes => opcoes
            .UseSqlite(conexaoBanco)
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
            CriarPastaDoArquivo(contexto.Database.GetConnectionString());
            await contexto.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private static void CriarPastaDoArquivo(string? conexao)
    {
        var configuracao = new SqliteConnectionStringBuilder(conexao);
        if (configuracao.Mode == SqliteOpenMode.Memory || string.IsNullOrWhiteSpace(configuracao.DataSource))
        {
            return;
        }

        var pasta = Path.GetDirectoryName(Path.GetFullPath(configuracao.DataSource));
        if (!string.IsNullOrEmpty(pasta))
        {
            Directory.CreateDirectory(pasta);
        }
    }
}
