# Borgonha Confeitaria — Backend

API REST para o sistema de gestão da Borgonha Confeitaria: PDV digital, controle de estoque e relatórios financeiros.

## Stack

| Camada | Tecnologia |
|--------|------------|
| Runtime | .NET (C#) |
| Banco de dados | MySQL |
| ORM | Entity Framework Core |
| API | REST / JSON |

## Módulos

### Produtos
- CRUD de produtos com nome, preço de venda e lista de ingredientes (receita)
- Cada produto tem uma receita que define quais ingredientes e quantidades são consumidos por unidade vendida

### PDV — Vendas
- Registro de venda: lista de `{ produtoId, quantidade }`
- Ao confirmar a venda: subtrai automaticamente o estoque de cada ingrediente conforme a receita de cada produto vendido
- Calcula total da venda e troco automaticamente
- Nenhuma venda pode ser finalizada se algum ingrediente estiver sem estoque suficiente

### Estoque de Ingredientes
- CRUD de ingredientes com nome, unidade de medida, quantidade atual e **quantidade mínima**
- Endpoint de alerta retorna todos os ingredientes com `quantidadeAtual <= quantidadeMinima`
- Movimentações de estoque são registradas (entrada manual / saída por venda)

### Relatórios Financeiros
- **Diário:** total de vendas, receita bruta, custo dos ingredientes utilizados, lucro
- **Mensal:** consolidado do mês com os mesmos indicadores + ranking dos produtos mais vendidos
- Lucro calculado como `receita bruta - custo total dos ingredientes consumidos nas vendas`

## Regras de Negócio

| Regra | Detalhe |
|-------|---------|
| Receita obrigatória | Produto não pode ser vendido sem ao menos 1 ingrediente na receita |
| Baixa de estoque automática | Toda venda aprovada desconta os ingredientes proporcionalmente |
| Bloqueio de venda | Venda rejeitada se qualquer ingrediente da receita estiver abaixo da quantidade necessária |
| Alerta de mínimo | Sistema expõe endpoint de ingredientes em nível crítico (`atual <= mínimo`) |
| Custo do produto | Calculado dinamicamente somando `(quantidade usada × custo unitário)` de cada ingrediente da receita |
| LGPD | Nenhum dado pessoal de cliente é coletado ou armazenado |

## Entidades Principais

```
Produto          { id, nome, precoVenda, ativo }
Ingrediente      { id, nome, unidade, quantidadeAtual, quantidadeMinima, custoUnitario }
ReceitaItem      { produtoId, ingredienteId, quantidade }
Venda            { id, dataHora, total, troco, valorPago }
ItemVenda        { vendaId, produtoId, quantidade, precoUnitario }
MovEstoque       { ingredienteId, tipo(entrada|saida), quantidade, dataHora, vendaId? }
```

## Endpoints (resumo)

```
GET    /produtos              Lista produtos ativos
POST   /produtos              Cria produto com receita
PUT    /produtos/{id}         Atualiza produto/receita
DELETE /produtos/{id}         Desativa produto

GET    /ingredientes          Lista ingredientes
GET    /ingredientes/alertas  Ingredientes em estoque mínimo
POST   /ingredientes          Cria ingrediente
PUT    /ingredientes/{id}     Atualiza ingrediente
POST   /ingredientes/{id}/entrada  Entrada manual de estoque

POST   /vendas                Registra venda (valida estoque e desconta)
GET    /vendas/{id}           Detalhe de venda

GET    /relatorios/diario?data=YYYY-MM-DD
GET    /relatorios/mensal?ano=YYYY&mes=MM
```

## Como rodar

```bash
# Pré-requisitos: .NET SDK, MySQL rodando

# 1. Configurar connection string em appsettings.Development.json
# 2. Aplicar migrations
dotnet ef database update

# 3. Subir a API
dotnet run
```

## Fora do Escopo

- Emissão de NF-e / NFC-e
- Integração com WhatsApp ou pedidos online
- Autenticação/login (MVP)
