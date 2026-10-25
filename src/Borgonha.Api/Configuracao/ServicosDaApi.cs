using Microsoft.AspNetCore.ResponseCompression;

namespace Borgonha.Api.Configuracao;

internal static class ServicosDaApi
{
    public static IServiceCollection AdicionarApi(
        this IServiceCollection services,
        IConfiguration configuracao,
        IHostEnvironment ambiente,
        string pastaDasChaves)
    {
        services.AddProblemDetails();
        services.AddExceptionHandler<TratadorDeExcecoes>();
        services.AddOpenApi();

        services.AddResponseCompression(opcoes =>
        {
            opcoes.EnableForHttps = true;
            opcoes.Providers.Add<BrotliCompressionProvider>();
            opcoes.Providers.Add<GzipCompressionProvider>();
        });

        services.AddOutputCache(opcoes =>
            opcoes.AddPolicy(PoliticasDeCache.Conteudo, politica => politica.Expire(TimeSpan.FromMinutes(5)).Tag(PoliticasDeCache.Conteudo)));

        services.AddOptions<CorsOpcoes>()
            .Bind(configuracao.GetSection(CorsOpcoes.Secao))
            .Validate(opcoes => opcoes.OrigensSaoValidas(), "Cors:OrigensPermitidas deve conter só origens absolutas, como https://www.seudominio.com.br.")
            .ValidateOnStart();

        // Com credenciais (o cookie da área do administrador), mas só para as origens configuradas.
        var cors = configuracao.GetSection(CorsOpcoes.Secao).Get<CorsOpcoes>() ?? new CorsOpcoes();
        services.AddCors(opcoes => opcoes.AddDefaultPolicy(politica => politica
            .WithOrigins([.. cors.OrigensPermitidas])
            .WithMethods(HttpMethods.Get, HttpMethods.Post, HttpMethods.Put, HttpMethods.Delete)
            .AllowAnyHeader()
            .AllowCredentials()));

        services.AdicionarAutenticacaoDoAdministrador(configuracao, ambiente, pastaDasChaves);
        services.AdicionarLimiteDeConsultasDeEntrega(configuracao);

        return services;
    }
}
