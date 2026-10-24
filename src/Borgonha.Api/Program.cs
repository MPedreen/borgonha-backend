using Borgonha.Api.Configuracao;
using Borgonha.Api.Endpoints;
using Borgonha.Aplicacao;
using Borgonha.Infraestrutura;

var builder = WebApplication.CreateBuilder(args);

var conexaoBanco = builder.Configuration.GetConnectionString("Borgonha")
    ?? throw new InvalidOperationException("Configure a connection string 'Borgonha'.");
var pastaDasImagens = PastaDaConfiguracao("Armazenamento:PastaDasImagens");

builder.Services
    .AdicionarAplicacao()
    .AdicionarInfraestrutura(conexaoBanco, pastaDasImagens)
    .AdicionarApi(builder.Configuration);

var app = builder.Build();

if (app.Configuration.GetValue("BancoDeDados:AplicarMigracoesNaInicializacao", defaultValue: true))
{
    await app.Services.AplicarMigracoesAsync();
}

app.UsarApi(pastaDasImagens);
app.MapearEndpoints();

await app.RunAsync();

// Pastas relativas são resolvidas a partir da pasta do projeto da API.
string PastaDaConfiguracao(string chave) =>
    Path.GetFullPath(
        builder.Configuration[chave] ?? throw new InvalidOperationException($"Configure {chave}."),
        builder.Environment.ContentRootPath);
