using Borgonha.Api.Configuracao;
using Borgonha.Api.Endpoints;
using Borgonha.Aplicacao;
using Borgonha.Infraestrutura;

var builder = WebApplication.CreateBuilder(args);

var conexaoBanco = builder.Configuration.GetConnectionString("Borgonha")
    ?? throw new InvalidOperationException("Configure a connection string 'Borgonha'.");

builder.Services
    .AdicionarAplicacao()
    .AdicionarInfraestrutura(conexaoBanco)
    .AdicionarApi(builder.Configuration);

var app = builder.Build();

if (app.Configuration.GetValue("BancoDeDados:AplicarMigracoesNaInicializacao", defaultValue: true))
{
    await app.Services.AplicarMigracoesAsync();
}

app.UsarApi();
app.MapearEndpoints();

await app.RunAsync();
