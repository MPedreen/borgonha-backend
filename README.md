# Borgonha Confeitaria — API

API do site da Borgonha Confeitaria (Santa Luzia, São Gonçalo – RJ): cardápio, dados da loja e dúvidas frequentes. O site fica no repositório [borgonha-frontend](https://github.com/MPedreen/borgonha-frontend).

.NET 10 LTS · Minimal APIs · Clean Architecture · EF Core 10 + PostgreSQL · xUnit v3

## Como rodar

Pré-requisitos: [.NET SDK 10](https://dotnet.microsoft.com/download/dotnet/10.0) (versão definida em `global.json`) e [Docker](https://www.docker.com/products/docker-desktop/), que roda o PostgreSQL.

```bash
# 1. Senha do banco: copie o modelo e troque a senha dentro do .env (o Git ignora esse arquivo)
cp .env.exemplo .env

# 2. Sobe o PostgreSQL em 127.0.0.1:5432
docker compose up -d

# 3. Entrega a connection string à API, fora do repositório (use a senha do .env)
dotnet user-secrets set "ConnectionStrings:Borgonha" "Host=localhost;Port=5432;Database=borgonha;Username=borgonha;Password=<senha do .env>" --project src/Borgonha.Api

# 4. Roda a API
dotnet run --project src/Borgonha.Api
```

Os passos 1 a 3 são só da primeira vez. Nas próximas, basta o Docker estar ligado: o banco sobe sozinho com ele, ou com `docker compose up -d`.

- API: http://localhost:5170
- Documentação interativa (Scalar): http://localhost:5170/scalar
- Exemplos de requisições: `src/Borgonha.Api/Borgonha.Api.http`

Na primeira execução, a API cria as tabelas (migrations) e cadastra o cardápio, a loja e as dúvidas (`src/Borgonha.Infraestrutura/Persistencia/DadosIniciais.cs`). Esses dados só entram com o banco vazio.

Nessa primeira partida o log mostra um `fail` do EF Core sobre a tabela `__EFMigrationsHistory`. É esperado: o EF consulta uma tabela que ainda não existe e, logo em seguida, aplica a migration.

## Endpoints

### Site

Todos são `GET` e respondem JSON. As rotas ficam 5 minutos em cache, e qualquer alteração feita na área do administrador limpa esse cache na hora.

| Rota | Retorna |
|---|---|
| `/api/catalogo` | Cardápio do dia, agrupado por categoria (cookies, doces e salgados). Produtos ocultos ficam de fora |
| `/api/catalogo/{slug}` | Um produto; 404 em ProblemDetails se não existir ou estiver oculto |
| `/api/loja` | Endereço, horários, Instagram, canais de pedido (WhatsApp, iFood e loja física) e a regra de entrega (`entrega`: raio sem pedido mínimo, em metros, e o valor do pedido mínimo) |
| `/api/duvidas` | Perguntas frequentes |
| `/api/entrega/enderecos?texto=` | Sugestões de endereço para o cliente conferir a entrega (mínimo de 4 letras). Não passa pelo cache do site; 503 se o serviço de mapas estiver indisponível |
| `/api/entrega/conferir?latitude=&longitude=` | Diz se a entrega no ponto tem pedido mínimo: `sem-minimo`, `com-minimo` ou `incerta`, com a distância. Mesmas regras do endereço acima |
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

### Entrega e pedido mínimo

A entrega é sempre grátis, a qualquer distância. Até 1,5 km da loja, **pelo trajeto de entrega**, não há pedido mínimo; mais longe, o pedido mínimo é de R$ 30 (sem limite de distância). O raio e o valor ficam na tabela `lojas` (ainda sem tela na área do administrador). Para mudar:

```sql
UPDATE lojas SET entrega_raio_sem_pedido_minimo_em_metros = 1500, entrega_pedido_minimo = 30;
```

Depois, atualize a dúvida "Tem pedido mínimo para entrega?" (tabela `duvidas`).

Para conferir o endereço do cliente, a API usa o [Geoapify](https://www.geoapify.com/) (plano gratuito: 3.000 créditos por dia, sem cartão): o `/api/entrega/enderecos` sugere endereços (autocompletar) e o `/api/entrega/conferir` calcula o trajeto. O navegador nunca fala com o Geoapify: a chave fica na API.

- **Chave:** crie a conta e a chave em geoapify.com. Em desenvolvimento, `dotnet user-secrets set "Geoapify:Chave" "<chave>" --project src/Borgonha.Api`; em produção, a variável de ambiente `Geoapify__Chave`. Nunca vai para o Git. Sem chave, a consulta responde "indisponível" (503) e o site pede para confirmar pelo WhatsApp.
- **Regra de decisão** (`RegraDeEntrega`): se a distância em linha reta já passa de raio + 250 m, o pedido mínimo vale sem gastar o cálculo do trajeto; senão, calcula-se o trajeto. Os mapas gratuitos estimam o número da casa com erro de algumas centenas de metros, então um trajeto a menos de 250 m do raio vira `incerta` ("confirme pelo WhatsApp").
- **Para não gastar crédito à toa:** cada endereço consultado fica 24 horas em cache (trajetos, 7 dias), o que vale para todos os clientes; o site só busca depois que a pessoa para de digitar e com pelo menos 4 letras; há um limite de 30 consultas por minuto por endereço IP (`Entrega:ConsultasPorMinutoPorEndereco`); e a API conta os créditos do dia (UTC) e para em 2.700 (`Geoapify:CreditosPorDia`), porque o Geoapify não bloqueia ao passar de 3.000: ele pede upgrade e pode suspender a conta. A contagem fica na memória e recomeça se a API reiniciar. Uma recusa 429 do Geoapify pausa as chamadas por 1 minuto.
- **Quando não dá para consultar** (sem chave, limite do dia, serviço fora do ar ou lento): 503 com a mensagem "A conferência de endereço está temporariamente indisponível. Fale com a gente pelo WhatsApp." O site mostra o aviso com o botão do WhatsApp e, enquanto isso, deixa procurar pelo nome da rua numa lista que vai junto com o site (estimativa em linha reta).
- **Privacidade e crédito:** os endereços digitados passam pelo Geoapify (por isso o log de `System.Net.Http.HttpClient` fica em Warning: a URL leva o endereço). O site mostra o crédito exigido pelo plano gratuito ("Geoapify" e "© colaboradores do OpenStreetMap"). Leia os termos de uso comercial do plano gratuito ao criar a conta.

### Pasta `dados/`

Guarda as fotos enviadas (`dados/imagens`) e as chaves que protegem o cookie da sessão (`dados/chaves`); o banco fica no PostgreSQL, fora dela. Precisa ser **persistente** na hospedagem e entrar no backup, junto com o banco. Nunca deve ir para o Git: as chaves permitiriam forjar uma sessão.

## Banco de dados

PostgreSQL 18, acessado pelo EF Core com o provedor Npgsql. Tabelas e colunas usam snake_case (`produtos`, `categoria_id`), então consultas escritas à mão não precisam de aspas.

- **Desenvolvimento:** o `docker-compose.yml` sobe o banco em `127.0.0.1:5432`, visível só nesta máquina. O usuário e o banco se chamam `borgonha`, e a senha vem do `.env`, que o Git ignora (o `.env.exemplo` mostra o formato). A API recebe a connection string pelos user-secrets, como o usuário do administrador: ela nunca fica no `appsettings.json` nem no Git.
- **Produção:** variável de ambiente `ConnectionStrings__Borgonha` (ou o cofre de segredos da hospedagem), com um banco e uma senha só da produção. Prefira o PostgreSQL gerenciado da hospedagem, que já faz backup.
- **Migrations:** a API aplica as que faltam ao iniciar (`BancoDeDados:AplicarMigracoesNaInicializacao`).

A senha do `.env` vale só na criação do banco. Para trocá-la depois, rode `ALTER USER borgonha PASSWORD '...'` no `psql` e atualize os user-secrets, ou recrie o banco como mostra abaixo.

```bash
docker compose up -d             # liga o banco
docker compose down              # desliga (os dados ficam guardados no volume)
docker compose down --volumes    # desliga e APAGA o banco: a API o recria do zero na próxima partida
docker compose exec banco psql --username=borgonha borgonha
```

### Backup

```bash
# Gera borgonha.dump nesta pasta
docker compose exec banco sh -c "pg_dump --username=borgonha --format=custom --file=/tmp/borgonha.dump borgonha"
docker compose cp banco:/tmp/borgonha.dump ./borgonha.dump

# Restaura, trocando o conteúdo atual do banco
docker compose cp ./borgonha.dump banco:/tmp/borgonha.dump
docker compose exec banco sh -c "pg_restore --username=borgonha --dbname=borgonha --clean --if-exists /tmp/borgonha.dump"
```

Guarde o `.dump` junto com a pasta `dados/`, fora do Git (o `.gitignore` já ignora `*.dump`).

## Organização

| Projeto | Responsabilidade |
|---|---|
| `src/Borgonha.Dominio` | Entidades e regras: catálogo, loja e dúvidas |
| `src/Borgonha.Aplicacao` | Casos de uso (consultas do site e edição do cardápio) e DTOs |
| `src/Borgonha.Infraestrutura` | EF Core + PostgreSQL (mapeamentos, migrations, repositórios e dados iniciais) e fotos otimizadas com SkiaSharp |
| `src/Borgonha.Api` | Endpoints, autenticação do administrador, ProblemDetails, CORS, cache, compressão, health check e OpenAPI |
| `tests/Borgonha.Dominio.Tests` | Testes das regras de domínio |
| `tests/Borgonha.Api.Tests` | Testes de integração (WebApplicationFactory + PostgreSQL em contêiner, via Testcontainers) |

## Testes e qualidade

```bash
dotnet test                         # testes de domínio e de integração
dotnet format --verify-no-changes   # formatação e estilo
```

Os testes de integração sobem um PostgreSQL descartável em contêiner (Testcontainers), então o Docker precisa estar ligado. Eles não tocam no banco de desenvolvimento.

O build trata avisos como erros (`Directory.Build.props`), e as versões dos pacotes ficam centralizadas em `Directory.Packages.props`. O CI (`.github/workflows/ci.yml`) roda formatação, build e testes a cada push na `main` e em cada pull request.

## Configuração

| Chave | Padrão | Para que serve |
|---|---|---|
| `ConnectionStrings:Borgonha` | nenhum (só user-secrets ou variável de ambiente) | Connection string do PostgreSQL |
| `Cors:OrigensPermitidas` | vazio (em desenvolvimento, `http://localhost:4200`) | Endereços do site que podem chamar a API |
| `BancoDeDados:AplicarMigracoesNaInicializacao` | `true` | Cria ou atualiza o banco ao iniciar |
| `Armazenamento:PastaDasImagens` | `dados/imagens` | Fotos enviadas pela área do administrador |
| `Armazenamento:PastaDasChaves` | `dados/chaves` | Chaves que protegem o cookie da sessão |
| `Geoapify:Chave` | nenhum (só user-secrets ou variável de ambiente) | Chave do Geoapify para a consulta de entrega |
| `Geoapify:CreditosPorDia` | `2700` | Créditos que a API pode gastar por dia (UTC); o plano gratuito dá 3.000 |
| `Entrega:ConsultasPorMinutoPorEndereco` | `30` | Consultas de entrega aceitas por minuto de cada endereço IP |
| `Administrador:Usuario` e `Administrador:SenhaHash` | nenhum (só user-secrets ou variáveis de ambiente) | Acesso à área do administrador |
| `Administrador:TentativasPorEndereco` / `TentativasPorHora` | `5` / `20` | Limites de tentativas de login |
| `Administrador:DuracaoDaSessao` | `08:00:00` | Tempo da sessão sem uso |

Pendente: definir as origens de produção do CORS quando o domínio do site estiver definido.

## Migrations

```bash
dotnet tool restore
dotnet ef migrations add NomeDaMigration --project src/Borgonha.Infraestrutura --startup-project src/Borgonha.Infraestrutura --output-dir Persistencia/Migracoes
```
