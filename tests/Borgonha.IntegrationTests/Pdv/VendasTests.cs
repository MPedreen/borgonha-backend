using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Borgonha.IntegrationTests.Infrastructure;
using Xunit;

namespace Borgonha.IntegrationTests.Pdv;

[Collection(nameof(BorgonhaCollection))]
public sealed class VendasTests : IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    private readonly BorgonhaWebAppFactory _factory;

    public VendasTests(BorgonhaWebAppFactory factory) => _factory = factory;

    public async Task InitializeAsync() => await _factory.ResetarBancoAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private HttpClient CriarCliente(string role)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, role);
        return client;
    }

    [Fact]
    public async Task RegistrarVenda_FluxoCompleto_Retorna201ComTotalEtrocoCorretos()
    {
        var admin = CriarCliente("admin");

        var ingredienteId = await SeedIngredienteAsync(admin, "Farinha", "g", quantidadeAtual: 1000m, custoUnitario: 0.10m);
        var produtoId = await SeedProdutoAsync(admin, "Bolo", precoVenda: 30m, ingredienteId, quantidadeReceita: 100m);

        var body = new
        {
            itens = new[] { new { produto_id = produtoId, quantidade = 2 } },
            valor_pago = 70m
        };

        var response = await admin.PostAsJsonAsync("/vendas", body, JsonOpts);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var root = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        Assert.Equal(60m, root.GetProperty("total").GetDecimal());    // 2 × R$30
        Assert.Equal(10m, root.GetProperty("troco").GetDecimal());    // R$70 - R$60
    }

    [Fact]
    public async Task RegistrarVenda_EstoqueInsuficiente_Retorna422()
    {
        var admin = CriarCliente("admin");

        // Ingrediente com apenas 50g, receita exige 100g por unidade
        var ingredienteId = await SeedIngredienteAsync(admin, "Açúcar", "g", quantidadeAtual: 50m, custoUnitario: 0.02m);
        var produtoId = await SeedProdutoAsync(admin, "Torta", precoVenda: 20m, ingredienteId, quantidadeReceita: 100m);

        var body = new
        {
            itens = new[] { new { produto_id = produtoId, quantidade = 1 } },
            valor_pago = 20m
        };

        var response = await admin.PostAsJsonAsync("/vendas", body, JsonOpts);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task RegistrarVenda_SemAutenticacao_Retorna401()
    {
        var anonimo = _factory.CreateClient();
        var body = new { itens = new[] { new { produto_id = Guid.NewGuid(), quantidade = 1 } }, valor_pago = 10m };

        var response = await anonimo.PostAsJsonAsync("/vendas", body, JsonOpts);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RegistrarVenda_ComoAtendente_Retorna201()
    {
        var admin = CriarCliente("admin");
        var atendente = CriarCliente("atendente");

        var ingredienteId = await SeedIngredienteAsync(admin, "Chocolate", "g", quantidadeAtual: 500m, custoUnitario: 0.05m);
        var produtoId = await SeedProdutoAsync(admin, "Brigadeiro", precoVenda: 5m, ingredienteId, quantidadeReceita: 20m);

        var body = new
        {
            itens = new[] { new { produto_id = produtoId, quantidade = 1 } },
            valor_pago = 5m
        };

        // Atendente pode registrar vendas
        var response = await atendente.PostAsJsonAsync("/vendas", body, JsonOpts);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task ObterVenda_VendaExistente_Retorna200ComDadosCorretos()
    {
        var admin = CriarCliente("admin");

        var ingredienteId = await SeedIngredienteAsync(admin, "Leite", "ml", quantidadeAtual: 1000m, custoUnitario: 0.005m);
        var produtoId = await SeedProdutoAsync(admin, "Vitamina", precoVenda: 12m, ingredienteId, quantidadeReceita: 200m);

        var bodyVenda = new
        {
            itens = new[] { new { produto_id = produtoId, quantidade = 1 } },
            valor_pago = 15m
        };
        var criarResp = await admin.PostAsJsonAsync("/vendas", bodyVenda, JsonOpts);
        Assert.Equal(HttpStatusCode.Created, criarResp.StatusCode);

        var vendaId = JsonDocument.Parse(await criarResp.Content.ReadAsStringAsync())
            .RootElement.GetProperty("id").GetGuid();

        var obterResp = await admin.GetAsync($"/vendas/{vendaId}");

        Assert.Equal(HttpStatusCode.OK, obterResp.StatusCode);
        var root = JsonDocument.Parse(await obterResp.Content.ReadAsStringAsync()).RootElement;
        Assert.Equal(vendaId, root.GetProperty("id").GetGuid());
        Assert.Equal(12m, root.GetProperty("total").GetDecimal());
        Assert.Equal(3m, root.GetProperty("troco").GetDecimal());
    }

    [Fact]
    public async Task ObterVenda_IdInexistente_Retorna404()
    {
        var admin = CriarCliente("admin");

        var response = await admin.GetAsync($"/vendas/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RegistrarVenda_BaixaEstoqueAposVenda_EstoqueReduzido()
    {
        var admin = CriarCliente("admin");

        var ingredienteId = await SeedIngredienteAsync(admin, "Manteiga", "g", quantidadeAtual: 500m, custoUnitario: 0.03m);
        var produtoId = await SeedProdutoAsync(admin, "Croissant", precoVenda: 8m, ingredienteId, quantidadeReceita: 50m);

        var body = new
        {
            itens = new[] { new { produto_id = produtoId, quantidade = 3 } }, // usa 150g
            valor_pago = 24m
        };
        var vendaResp = await admin.PostAsJsonAsync("/vendas", body, JsonOpts);
        Assert.Equal(HttpStatusCode.Created, vendaResp.StatusCode);

        // Verificar que 350g restam (500 - 3×50)
        var ingredientesResp = await admin.GetAsync("/ingredientes");
        Assert.Equal(HttpStatusCode.OK, ingredientesResp.StatusCode);
        var ingredientes = JsonDocument.Parse(await ingredientesResp.Content.ReadAsStringAsync()).RootElement;
        var manteiga = ingredientes.EnumerateArray()
            .Single(i => i.GetProperty("id").GetGuid() == ingredienteId);
        Assert.Equal(350m, manteiga.GetProperty("quantidade_atual").GetDecimal());
    }

    // --- Helpers ---

    private static async Task<Guid> SeedIngredienteAsync(
        HttpClient client, string nome, string unidade,
        decimal quantidadeAtual, decimal custoUnitario)
    {
        var body = new
        {
            nome,
            unidade,
            quantidade_atual = quantidadeAtual,
            quantidade_minima = 0m,
            custo_unitario = custoUnitario
        };
        var response = await client.PostAsJsonAsync("/ingredientes", body, JsonOpts);
        response.EnsureSuccessStatusCode();
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync())
            .RootElement.GetProperty("id").GetGuid();
    }

    private static async Task<Guid> SeedProdutoAsync(
        HttpClient client, string nome, decimal precoVenda,
        Guid ingredienteId, decimal quantidadeReceita)
    {
        var body = new
        {
            nome,
            preco_venda = precoVenda,
            receita = new[] { new { ingrediente_id = ingredienteId, quantidade = quantidadeReceita } }
        };
        var response = await client.PostAsJsonAsync("/produtos", body, JsonOpts);
        response.EnsureSuccessStatusCode();
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync())
            .RootElement.GetProperty("id").GetGuid();
    }
}
