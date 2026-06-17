# Borgonha Confeitaria — Backend

API REST para o sistema de gestão da Borgonha Confeitaria: PDV digital, controle de estoque e relatórios financeiros.

## Stack

| Camada | Tecnologia |
|--------|------------|
| Runtime | .NET 8 (C#) |
| Banco de dados | PostgreSQL 16 |
| ORM / Queries | EF Core 8 (CRUD) + Dapper (relatórios) |
| Autenticação | Keycloak 24 (JWT Bearer) |

## Arquitetura

Estrutura de 4 camadas:

```
src/
├── Borgonha.Api/           # Controllers, Program.cs, middlewares
├── Borgonha.Domain/        # Entidades, regras de negócio, interfaces de repositório
├── Borgonha.Service/       # Orquestração de casos de uso
└── Borgonha.Infrastructure/ # EF Core, Dapper, implementações de repositório
```

## Endpoints

### Produtos — `admin`

| Método | Rota | Descrição |
|--------|------|-----------|
| `GET` | `/produtos` | Lista produtos ativos (`admin` e `atendente`) |
| `POST` | `/produtos` | Cria produto com receita |
| `PUT` | `/produtos/{id}` | Atualiza produto / ativa ou desativa |

### Ingredientes — `admin`

| Método | Rota | Descrição |
|--------|------|-----------|
| `GET` | `/ingredientes` | Lista todos os ingredientes |
| `GET` | `/ingredientes/alertas` | Ingredientes com estoque ≤ mínimo |
| `POST` | `/ingredientes` | Cria ingrediente |
| `PUT` | `/ingredientes/{id}` | Atualiza ingrediente |
| `PATCH` | `/ingredientes/{id}/entrada` | Registra entrada manual de estoque |

### PDV — `admin` e `atendente`

| Método | Rota | Descrição |
|--------|------|-----------|
| `POST` | `/vendas` | Registra venda (valida estoque e desconta ingredientes) |
| `GET` | `/vendas/{id}` | Detalhe de venda (recibo) |

### Relatórios — `admin`

| Método | Rota | Descrição |
|--------|------|-----------|
| `GET` | `/relatorios/diario?data=YYYY-MM-DD` | KPIs do dia |
| `GET` | `/relatorios/mensal?ano=YYYY&mes=MM` | KPIs do mês + ranking de produtos |

## Regras de negócio principais

| Regra | Detalhe |
|-------|---------|
| Receita obrigatória | Produto precisa de ao menos 1 ingrediente na receita para ser criado |
| Validação antecipada | Todos os itens do carrinho são validados antes de qualquer baixa de estoque |
| Baixa atômica | Desconto dos ingredientes ocorre em transação única com o registro da venda |
| Alerta de mínimo | Ingredientes com `quantidade_atual ≤ quantidade_minima` aparecem em `/alertas` |
| Relatórios via Dapper | Queries analíticas (JOIN + GROUP BY + SUM) usam Dapper, não EF Core |

## Como rodar

### Com Docker (recomendado)

```bash
# 1. Copiar e preencher variáveis de ambiente (na raiz do monorepo)
cp .env.example .env

# 2. Subir infraestrutura (postgres + keycloak)
docker compose up -d

# 3. Subir backend + frontend
docker compose --profile app up -d --build
```

A API ficará disponível em `http://localhost:5000`.

### Localmente

```bash
# Pré-requisitos: .NET 8 SDK, PostgreSQL rodando, Keycloak rodando

# 1. Configurar appsettings.Development.json com connection string e Keycloak
# 2. Aplicar migrations
dotnet ef database update --project src/Borgonha.Infrastructure --startup-project src/Borgonha.Api

# 3. Subir a API
dotnet run --project src/Borgonha.Api
```

## Variáveis de ambiente

| Variável | Descrição |
|----------|-----------|
| `ConnectionStrings__Default` | Connection string do PostgreSQL |
| `Keycloak__Authority` | URL pública do realm (ex: `http://localhost:8080/realms/borgonha`) |
| `Keycloak__Audience` | Client ID do backend (`borgonha-backend`) |
| `Keycloak__MetadataAddress` | URL interna para discovery JWKS (apenas em Docker) |
