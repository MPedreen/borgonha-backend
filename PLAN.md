# Plano de Implementação — Backend

Sistema de gestão da Borgonha Confeitaria: PDV, estoque e relatórios financeiros.

---

## Stack

| Camada | Tecnologia | Versão |
|--------|------------|--------|
| Runtime | .NET | 8 |
| Banco de dados | PostgreSQL | 16 |
| ORM (escrita + migrações) | Entity Framework Core | 8 |
| Queries de performance | Dapper + Npgsql | latest |
| Autenticação | Keycloak (JWT Bearer) | 24 |
| Contêineres | Docker / docker-compose | — |

---

## Arquitetura

```
borgonha-backend/
├── src/
│   ├── Api/                        # Controllers, Program.cs, middleware
│   ├── Application/                # Services, DTOs, interfaces
│   ├── Domain/                     # Entidades, enums, regras de negócio puras
│   └── Infrastructure/
│       ├── Persistence/
│       │   ├── EfCore/             # DbContext, migrations, configs de mapeamento
│       │   └── Dapper/             # Queries SQL, connection factory
│       └── Auth/                   # Keycloak JWT validation setup
├── docker-compose.yml
└── .env.example
```

**Fluxo por camada:** `Controller → Service → Repository (EF Core ou Dapper)`

---

## Critério EF Core vs Dapper

| Operação | Usar |
|----------|------|
| INSERT / UPDATE / DELETE | EF Core |
| Leitura simples (por ID, lista ativa) | EF Core |
| Migrações e schema | EF Core |
| Relatório diário/mensal (JOIN + GROUP BY + SUM) | Dapper |
| Ranking de produtos mais vendidos | Dapper |
| Histórico paginado de movimentações de estoque | Dapper |
| Consulta de ingredientes em alerta | EF Core (filtro simples) |

---

## Modelo de Dados

```sql
-- Produtos
produtos (
  id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  nome          VARCHAR(100) NOT NULL,
  preco_venda   NUMERIC(10,2) NOT NULL,
  ativo         BOOLEAN NOT NULL DEFAULT true,
  criado_em     TIMESTAMPTZ NOT NULL DEFAULT now()
)

-- Ingredientes
ingredientes (
  id                UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  nome              VARCHAR(100) NOT NULL,
  unidade           VARCHAR(20) NOT NULL,   -- ex: "g", "ml", "un"
  quantidade_atual  NUMERIC(10,3) NOT NULL DEFAULT 0,
  quantidade_minima NUMERIC(10,3) NOT NULL DEFAULT 0,
  custo_unitario    NUMERIC(10,4) NOT NULL DEFAULT 0,
  criado_em         TIMESTAMPTZ NOT NULL DEFAULT now()
)

-- Receita (ingredientes por produto)
receita_itens (
  id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  produto_id      UUID NOT NULL REFERENCES produtos(id),
  ingrediente_id  UUID NOT NULL REFERENCES ingredientes(id),
  quantidade      NUMERIC(10,3) NOT NULL,
  UNIQUE (produto_id, ingrediente_id)
)

-- Vendas
vendas (
  id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  data_hora   TIMESTAMPTZ NOT NULL DEFAULT now(),
  total       NUMERIC(10,2) NOT NULL,
  valor_pago  NUMERIC(10,2) NOT NULL,
  troco       NUMERIC(10,2) NOT NULL,
  criado_por  VARCHAR(100) NOT NULL  -- username vindo do JWT
)

-- Itens de cada venda
itens_venda (
  id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  venda_id        UUID NOT NULL REFERENCES vendas(id),
  produto_id      UUID NOT NULL REFERENCES produtos(id),
  quantidade      INTEGER NOT NULL,
  preco_unitario  NUMERIC(10,2) NOT NULL
)

-- Movimentações de estoque
movimentacoes_estoque (
  id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  ingrediente_id  UUID NOT NULL REFERENCES ingredientes(id),
  tipo            VARCHAR(10) NOT NULL CHECK (tipo IN ('entrada','saida')),
  quantidade      NUMERIC(10,3) NOT NULL,
  data_hora       TIMESTAMPTZ NOT NULL DEFAULT now(),
  venda_id        UUID REFERENCES vendas(id),  -- NULL para entradas manuais
  observacao      TEXT
)
```

---

## Regras de Negócio

### PDV / Vendas
1. Ao receber `POST /vendas`, verificar se cada `ingrediente.quantidade_atual >= soma(receita_item.quantidade * item.quantidade)` para todos os itens do carrinho. Se qualquer ingrediente estiver insuficiente → retornar `422` com lista de ingredientes bloqueantes.
2. Se estoque ok → persistir `Venda` + `ItensVenda` + baixar `quantidade_atual` de cada ingrediente + registrar `MovimentacaoEstoque` com `tipo = 'saida'` e `venda_id` vinculado. Tudo em uma única transação.
3. `troco = valor_pago - total`. Retornar erro se `valor_pago < total`.

### Estoque
4. `PATCH /ingredientes/{id}/entrada` registra `MovimentacaoEstoque` com `tipo = 'entrada'` e incrementa `quantidade_atual`. Operação atômica.
5. `GET /ingredientes/alertas` retorna ingredientes onde `quantidade_atual <= quantidade_minima`.

### Produtos
6. Produto não pode ser vendido sem ao menos 1 item na receita — validar no `POST /vendas` e no `PUT /produtos/{id}`.
7. Desativar produto (`ativo = false`) não remove do histórico de vendas.

### Custo de produto
8. Custo calculado em tempo de consulta: `SUM(ri.quantidade * i.custo_unitario)` via Dapper. Nunca persistido (evita inconsistência com custo_unitario do ingrediente).

---

## Autenticação e Autorização (Keycloak)

### Roles
| Role Keycloak | Persona | Permissões |
|---------------|---------|------------|
| `admin` | Bruna (proprietária) | Tudo: CRUD produtos, ingredientes, relatórios, vendas |
| `atendente` | Laura | `GET /produtos`, `POST /vendas` apenas |

### Configuração
```csharp
// Program.cs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.Authority = "http://keycloak:8080/realms/borgonha";
        options.Audience = "borgonha-backend";
        options.RequireHttpsMetadata = false; // false apenas em dev
    });
```

O token JWT vindo do Keycloak já carrega as roles em `realm_access.roles`. Mapear via policy:

```csharp
builder.Services.AddAuthorization(options => {
    options.AddPolicy("AdminOnly", p => p.RequireRole("admin"));
    options.AddPolicy("AtendentePlus", p => p.RequireRole("admin", "atendente"));
});
```

---

## Docker Compose

```yaml
# docker-compose.yml
services:
  postgres:
    image: postgres:16-alpine
    environment:
      POSTGRES_DB: borgonha
      POSTGRES_USER: borgonha
      POSTGRES_PASSWORD: ${DB_PASSWORD}
    volumes:
      - postgres_data:/var/lib/postgresql/data
    ports:
      - "5432:5432"

  keycloak:
    image: quay.io/keycloak/keycloak:24.0
    command: start-dev --import-realm
    environment:
      KC_DB: postgres
      KC_DB_URL: jdbc:postgresql://postgres:5432/borgonha
      KC_DB_USERNAME: borgonha
      KC_DB_PASSWORD: ${DB_PASSWORD}
      KEYCLOAK_ADMIN: admin
      KEYCLOAK_ADMIN_PASSWORD: ${KEYCLOAK_ADMIN_PASSWORD}
    volumes:
      - ./keycloak/realm-export.json:/opt/keycloak/data/import/realm.json
    ports:
      - "8080:8080"
    depends_on:
      - postgres

  backend:
    build: .
    environment:
      ConnectionStrings__Default: "Host=postgres;Database=borgonha;Username=borgonha;Password=${DB_PASSWORD}"
      Keycloak__Authority: "http://keycloak:8080/realms/borgonha"
    ports:
      - "5000:8080"
    depends_on:
      - postgres
      - keycloak

volumes:
  postgres_data:
```

Criar `keycloak/realm-export.json` com realm `borgonha`, client `borgonha-backend`, client `borgonha-frontend` e roles `admin` / `atendente`.

---

## Queries Dapper (exemplos)

### Relatório diário
```sql
SELECT
  COUNT(DISTINCT v.id)                        AS total_vendas,
  SUM(v.total)                                AS receita_bruta,
  SUM(iv.quantidade * ri.quantidade * i.custo_unitario) AS custo_total,
  SUM(v.total) - SUM(iv.quantidade * ri.quantidade * i.custo_unitario) AS lucro
FROM vendas v
JOIN itens_venda iv ON iv.venda_id = v.id
JOIN receita_itens ri ON ri.produto_id = iv.produto_id
JOIN ingredientes i ON i.id = ri.ingrediente_id
WHERE v.data_hora::date = @Data
```

### Ranking de produtos (mensal)
```sql
SELECT
  p.nome,
  SUM(iv.quantidade)     AS unidades_vendidas,
  SUM(iv.quantidade * iv.preco_unitario) AS receita
FROM itens_venda iv
JOIN vendas v ON v.id = iv.venda_id
JOIN produtos p ON p.id = iv.produto_id
WHERE DATE_TRUNC('month', v.data_hora) = DATE_TRUNC('month', @Referencia::date)
GROUP BY p.id, p.nome
ORDER BY unidades_vendidas DESC
```

---

## Sprint Plan

### Sprint 1 — Infra, Modelo e Auth (dias 1–7)
- [ ] Estrutura do projeto (`dotnet new webapi`, pastas de camadas)
- [ ] `docker-compose.yml` com postgres + keycloak
- [ ] `DbContext` + mapeamentos EF Core de todas as entidades
- [ ] Migration inicial (schema completo)
- [ ] Keycloak: realm `borgonha`, clients, roles `admin`/`atendente`
- [ ] Configurar `AddAuthentication` + policies no `Program.cs`
- [ ] Dapper `ConnectionFactory` (usa a mesma connection string do EF Core)
- [ ] `GET /health` e smoke test da autenticação

### Sprint 2 — PDV, Produtos e Estoque (dias 8–14)
- [ ] `POST /produtos`, `GET /produtos`, `PUT /produtos/{id}` (EF Core)
- [ ] `POST /ingredientes`, `GET /ingredientes`, `PUT /ingredientes/{id}` (EF Core)
- [ ] `GET /ingredientes/alertas` (EF Core)
- [ ] `PATCH /ingredientes/{id}/entrada` com `MovimentacaoEstoque` (EF Core, transação)
- [ ] `POST /vendas` — validação de estoque + baixa + movimentação (EF Core, transação)
- [ ] `GET /vendas/{id}`
- [ ] Autorização por role em todos os endpoints

### Sprint 3 — Relatórios, Ajustes e Entrega (dias 15–21)
- [ ] `GET /relatorios/diario?data=` (Dapper)
- [ ] `GET /relatorios/mensal?ano=&mes=` (Dapper)
- [ ] `GET /movimentacoes?ingredienteId=&page=&size=` (Dapper, paginado)
- [ ] Testes de integração nos endpoints críticos (PDV + relatórios)
- [ ] Variáveis de ambiente no docker-compose + `.env.example`
- [ ] README atualizado com endpoints e como rodar
