# Borgonha Confeitaria — API

API do site da Borgonha Confeitaria (Santa Luzia, São Gonçalo – RJ): cardápio, dados da loja e dúvidas frequentes. O site fica no repositório [borgonha-frontend](https://github.com/MPedreen/borgonha-frontend).

.NET 10 LTS · Minimal APIs · Clean Architecture · EF Core 10 + SQLite · xUnit v3

## Como rodar

Pré-requisito: [.NET SDK 10](https://dotnet.microsoft.com/download/dotnet/10.0) (versão definida em `global.json`).

```bash
dotnet run --project src/Borgonha.Api
```

- API: http://localhost:5170
- Documentação interativa (Scalar): http://localhost:5170/scalar
- Exemplos de requisições: `src/Borgonha.Api/Borgonha.Api.http`

Na primeira execução, a API cria o banco SQLite em `src/Borgonha.Api/dados/borgonha.db`, já com o cardápio, a loja e as dúvidas (`src/Borgonha.Infraestrutura/Persistencia/DadosIniciais.cs`). Esses dados só entram com o banco vazio: para recriá-lo, apague o arquivo.

## Endpoints

Todos são `GET` e respondem JSON. As rotas em `/api` ficam 5 minutos em cache.

| Rota | Retorna |
|---|---|
| `/api/catalogo` | Cardápio completo, agrupado por categoria (cookies, doces e salgados) |
| `/api/catalogo/{slug}` | Um produto; 404 em ProblemDetails se não existir |
| `/api/loja` | Endereço, horários, Instagram e canais de pedido (WhatsApp, iFood e loja física) |
| `/api/duvidas` | Perguntas frequentes |
| `/health` | Saúde da API e do banco |

## Organização

| Projeto | Responsabilidade |
|---|---|
| `src/Borgonha.Dominio` | Entidades e regras: catálogo, loja e dúvidas |
| `src/Borgonha.Aplicacao` | Consultas (casos de uso) e DTOs |
| `src/Borgonha.Infraestrutura` | EF Core + SQLite: mapeamentos, migrations, repositórios e dados iniciais |
| `src/Borgonha.Api` | Endpoints, ProblemDetails, CORS, cache, compressão, health check e OpenAPI |
| `tests/Borgonha.Dominio.Tests` | Testes das regras de domínio |
| `tests/Borgonha.Api.Tests` | Testes de integração (WebApplicationFactory + SQLite em memória) |

## Testes e qualidade

```bash
dotnet test                         # testes de domínio e de integração
dotnet format --verify-no-changes   # formatação e estilo
```

O build trata avisos como erros (`Directory.Build.props`), e as versões dos pacotes ficam centralizadas em `Directory.Packages.props`. O CI (`.github/workflows/ci.yml`) roda formatação, build e testes a cada push na `main` e em cada pull request.

## Configuração

| Chave | Padrão | Para que serve |
|---|---|---|
| `ConnectionStrings:Borgonha` | `Data Source=dados/borgonha.db` | Banco SQLite |
| `Cors:OrigensPermitidas` | vazio (em desenvolvimento, `http://localhost:4200`) | Endereços do site que podem chamar a API |
| `BancoDeDados:AplicarMigracoesNaInicializacao` | `true` | Cria ou atualiza o banco ao iniciar |

Pendente: definir as origens de produção do CORS quando o domínio do site estiver definido.

## Migrations

```bash
dotnet tool restore
dotnet ef migrations add NomeDaMigration --project src/Borgonha.Infraestrutura --startup-project src/Borgonha.Infraestrutura --output-dir Persistencia/Migracoes
```
