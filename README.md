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

### Site

Todos são `GET` e respondem JSON. As rotas ficam 5 minutos em cache, e qualquer alteração feita na área do administrador limpa esse cache na hora.

| Rota | Retorna |
|---|---|
| `/api/catalogo` | Cardápio do dia, agrupado por categoria (cookies, doces e salgados). Produtos ocultos ficam de fora |
| `/api/catalogo/{slug}` | Um produto; 404 em ProblemDetails se não existir ou estiver oculto |
| `/api/loja` | Endereço, horários, Instagram e canais de pedido (WhatsApp, iFood e loja física) |
| `/api/duvidas` | Perguntas frequentes |
| `/api/imagens/envios/{arquivo}.webp` | Fotos enviadas pela área do administrador |
| `/health` | Saúde da API e do banco |

### Área do administrador

Exigem a sessão aberta (cookie) e o cabeçalho `X-Borgonha-Admin`. Respostas com `Cache-Control: no-store`.

| Rota | O que faz |
|---|---|
| `POST /api/admin/sessao` | Entra com `{ usuario, senha }` e abre a sessão |
| `GET /api/admin/sessao` | 204 com a sessão aberta; 401 sem ela |
| `DELETE /api/admin/sessao` | Sai |
| `GET /api/admin/cardapio` | Todos os produtos, inclusive os ocultos, com a situação de cada um |
| `POST /api/admin/produtos` | Cadastra um produto (o slug sai do nome; só o nome é obrigatório) |
| `PUT /api/admin/produtos/{slug}` | Troca nome, descrição, frase, gramatura, preço e variações |
| `PUT /api/admin/produtos/{slug}/situacao` | `no-cardapio`, `esgotado` (aparece com o aviso) ou `oculto` (sai do site) |
| `PUT /api/admin/produtos/{slug}/fotos/{inteiro\|partido}` | Envia uma foto (JPG, PNG, WebP ou HEIC, até 15 MB) |
| `DELETE /api/admin/produtos/{slug}/fotos/{inteiro\|partido}` | Tira a foto |
| `DELETE /api/admin/produtos/{slug}` | Apaga o produto e as fotos enviadas para ele |

## Área do administrador: usuário e senha

O usuário e a senha **nunca** ficam no código, no `appsettings.json` nem no Git (os repositórios são públicos). A API guarda só o usuário e um **hash** da senha (PBKDF2-SHA256, 600 mil iterações, sal aleatório), lidos da configuração:

- **Desenvolvimento:** [user-secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets), um arquivo fora do repositório, no perfil do usuário do Windows.
- **Produção:** variáveis de ambiente `Administrador__Usuario` e `Administrador__SenhaHash`, ou o cofre de segredos da hospedagem.

Para configurar ou **trocar a senha**:

```bash
# Pede a senha sem mostrá-la na tela e imprime o hash (a senha não é gravada em lugar nenhum)
dotnet run --project src/Borgonha.Api -- gerar-hash-senha

dotnet user-secrets set "Administrador:Usuario" "<usuário>" --project src/Borgonha.Api
dotnet user-secrets set "Administrador:SenhaHash" "<hash impresso acima>" --project src/Borgonha.Api
```

Ao trocar o hash, as sessões abertas com a senha antiga deixam de valer. Sem usuário e hash configurados, a API funciona normalmente e só a área do administrador fica fechada.

Outras proteções:

- **Sessão:** cookie `HttpOnly`, `SameSite=Strict` e, fora do desenvolvimento, `Secure` com o prefixo `__Host-`; dura 8 horas, renovadas a cada uso.
- **Força bruta:** no máximo 5 tentativas de login a cada 15 minutos por endereço IP e 20 por hora no total. Usuário e senha são conferidos em tempo constante, e a resposta de erro é a mesma para usuário ou senha errados. Atrás de um proxy reverso, configure os cabeçalhos `X-Forwarded-For` para o limite por IP enxergar o endereço real.
- **CSRF:** além do `SameSite=Strict`, toda chamada exige o cabeçalho `X-Borgonha-Admin`, que outros sites não conseguem enviar (o CORS só libera as origens configuradas).
- **Fotos:** a API abre e regrava cada foto em WebP. Arquivos que não são imagem são recusados, e metadados como a localização do celular não chegam ao site.

### Pasta `dados/`

Guarda o banco, as fotos enviadas (`dados/imagens`) e as chaves que protegem o cookie da sessão (`dados/chaves`). Precisa ser **persistente** na hospedagem e entrar no backup. Nunca deve ir para o Git: as chaves permitiriam forjar uma sessão.

## Organização

| Projeto | Responsabilidade |
|---|---|
| `src/Borgonha.Dominio` | Entidades e regras: catálogo, loja e dúvidas |
| `src/Borgonha.Aplicacao` | Casos de uso (consultas do site e edição do cardápio) e DTOs |
| `src/Borgonha.Infraestrutura` | EF Core + SQLite (mapeamentos, migrations, repositórios e dados iniciais) e fotos otimizadas com SkiaSharp |
| `src/Borgonha.Api` | Endpoints, autenticação do administrador, ProblemDetails, CORS, cache, compressão, health check e OpenAPI |
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
| `Armazenamento:PastaDasImagens` | `dados/imagens` | Fotos enviadas pela área do administrador |
| `Armazenamento:PastaDasChaves` | `dados/chaves` | Chaves que protegem o cookie da sessão |
| `Administrador:Usuario` e `Administrador:SenhaHash` | nenhum (só user-secrets ou variáveis de ambiente) | Acesso à área do administrador |
| `Administrador:TentativasPorEndereco` / `TentativasPorHora` | `5` / `20` | Limites de tentativas de login |
| `Administrador:DuracaoDaSessao` | `08:00:00` | Tempo da sessão sem uso |

Pendente: definir as origens de produção do CORS quando o domínio do site estiver definido.

## Migrations

```bash
dotnet tool restore
dotnet ef migrations add NomeDaMigration --project src/Borgonha.Infraestrutura --startup-project src/Borgonha.Infraestrutura --output-dir Persistencia/Migracoes
```
