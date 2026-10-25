using Borgonha.Api.Administracao;
using Borgonha.Api.Configuracao;
using Borgonha.Api.Endpoints;
using Borgonha.Aplicacao;
using Borgonha.Infraestrutura;

// dotnet run --project src/Borgonha.Api -- gerar-hash-senha
if (args is [GeradorDeHashDeSenha.Comando])
{
    return GeradorDeHashDeSenha.Executar();
}

var builder = WebApplication.CreateBuilder(args);

var conexaoBanco = builder.Configuration.GetConnectionString("Borgonha")
    ?? throw new InvalidOperationException("Configure a connection string 'Borgonha' do PostgreSQL (veja o README).");
var pastaDasImagens = PastaDaConfiguracao("Armazenamento:PastaDasImagens");
var pastaDasChaves = PastaDaConfiguracao("Armazenamento:PastaDasChaves");

builder.Services
    .AdicionarAplicacao()
    .AdicionarInfraestrutura(conexaoBanco, pastaDasImagens)
    .AdicionarApi(builder.Configuration, builder.Environment, pastaDasChaves);

var app = builder.Build();

if (app.Configuration.GetValue("BancoDeDados:AplicarMigracoesNaInicializacao", defaultValue: true))
{
    await app.Services.AplicarMigracoesAsync();
}

app.UsarApi(pastaDasImagens);
app.MapearEndpoints();

await app.RunAsync();
return 0;

// Pastas relativas são resolvidas a partir da pasta do projeto da API.
string PastaDaConfiguracao(string chave) =>
    Path.GetFullPath(
        builder.Configuration[chave] ?? throw new InvalidOperationException($"Configure {chave}."),
        builder.Environment.ContentRootPath);
