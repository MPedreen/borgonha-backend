using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Borgonha.IntegrationTests.Infrastructure;
using Xunit;

namespace Borgonha.IntegrationTests.Relatorios;

[Collection(nameof(BorgonhaCollection))]
public sealed class RelatoriosTests : IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    private readonly BorgonhaWebAppFactory _factory;

    public RelatoriosTests(BorgonhaWebAppFactory factory) => _factory = factory;

    public async Task InitializeAsync() => await _factory.ResetarBancoAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private HttpClient CriarCliente(string role)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, role);
        return client;
    }

    [Fact]
    public async Task ObterDiario_SemVendas_RetornaZeros()
    {
        var admin = CriarCliente("admin");
        var data = DateTime.UtcNow.ToString("yyyy-MM-dd");

        var response = await admin.GetAsync($"/relatorios/diario?data={data}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var root = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        Assert.Equal(0, root.GetProperty("total_vendas").GetInt32());
        Assert.Equal(0m, root.GetProperty("receita_bruta").GetDecimal());
        Assert.Equal(0m, root.GetProperty("custo_total").GetDecimal());
        Assert.Equal(0m, root.GetProperty("lucro").GetDecimal());
    }

    [Fact]
    public async Task ObterDiario_ComVendas_RetornaKpisCorretos()
    {
        var admin = CriarCliente("admin");

        // Ingrediente: 1000g, custo R$0.10/g
        // Produto: R$30, receita 100g → custo unitário R$10
        var ingredienteId = await SeedIngredienteAsync(admin, "Chocolate", "g", quantidadeAtual: 1000m, custoUnitario: 0.10m);
        var produtoId = await SeedProdutoAsync(admin, "Brigadeiro", precoVenda: 30m, ingredienteId, quantidadeReceita: 100m);

        // 2 vendas de 1 unidade cada
        await SeedVendaAsync(admin, produtoId, quantidade: 1, valorPago: 30m);
        await SeedVendaAsync(admin, produtoId, quantidade: 1, valorPago: 30m);

        var data = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var response = await admin.GetAsync($"/relatorios/diario?data={data}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var root = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

        Assert.Equal(2, root.GetProperty("total_vendas").GetInt32());
        Assert.Equal(60m, root.GetProperty("receita_bruta").GetDecimal());  // 2 × R$30
        Assert.Equal(20m, root.GetProperty("custo_total").GetDecimal());    // 2 × 100g × R$0.10
        Assert.Equal(40m, root.GetProperty("lucro").GetDecimal());          // R$60 - R$20
    }

    [Fact]
    public async Task ObterMensal_ComVendas_RetornaKpisERankingOrdenado()
    {
        var admin = CriarCliente("admin");

        var ing1 = await SeedIngredienteAsync(admin, "Leite", "ml", quantidadeAtual: 5000m, custoUnitario: 0.005m);
        var ing2 = await SeedIngredienteAsync(admin, "Ovos", "un", quantidadeAtual: 50m, custoUnitario: 0.80m);

        // Produto 1: Pudim R$25 (receita: 200ml leite)
        var pudimId = await SeedProdutoAsync(admin, "Pudim", precoVenda: 25m, ing1, quantidadeReceita: 200m);
        // Produto 2: Flan R$20 (receita: 2 ovos)
        var flanId = await SeedProdutoAsync(admin, "Flan", precoVenda: 20m, ing2, quantidadeReceita: 2m);

        // 3 vendas do Pudim, 1 do Flan
        await SeedVendaAsync(admin, pudimId, quantidade: 1, valorPago: 25m);
        await SeedVendaAsync(admin, pudimId, quantidade: 1, valorPago: 25m);
        await SeedVendaAsync(admin, pudimId, quantidade: 1, valorPago: 25m);
        await SeedVendaAsync(admin, flanId, quantidade: 1, valorPago: 20m);

        var ano = DateTime.UtcNow.Year;
        var mes = DateTime.UtcNow.Month;
        var response = await admin.GetAsync($"/relatorios/mensal?ano={ano}&mes={mes}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var root = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

        Assert.Equal(4, root.GetProperty("total_vendas").GetInt32());
        Assert.Equal(95m, root.GetProperty("receita_bruta").GetDecimal()); // 3×25 + 1×20

        var ranking = root.GetProperty("ranking");
        Assert.Equal(2, ranking.GetArrayLength());

        // Pudim deve vir primeiro (3 unidades > 1 unidade)
        Assert.Equal("Pudim", ranking[0].GetProperty("nome").GetString());
        Assert.Equal(3, ranking[0].GetProperty("unidades_vendidas").GetInt32());
        Assert.Equal(75m, ranking[0].GetProperty("receita").GetDecimal());

        Assert.Equal("Flan", ranking[1].GetProperty("nome").GetString());
    }

    [Fact]
    public async Task ObterDiario_ComoAtendente_Retorna403()
    {
        var atendente = CriarCliente("atendente");
        var data = DateTime.UtcNow.ToString("yyyy-MM-dd");

        var response = await atendente.GetAsync($"/relatorios/diario?data={data}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ObterMensal_SemVendas_RetornaZerosERankingVazio()
    {
        var admin = CriarCliente("admin");
        var ano = DateTime.UtcNow.Year;
        var mes = DateTime.UtcNow.Month;

        var response = await admin.GetAsync($"/relatorios/mensal?ano={ano}&mes={mes}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var root = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        Assert.Equal(0, root.GetProperty("total_vendas").GetInt32());
        Assert.Equal(0, root.GetProperty("ranking").GetArrayLength());
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

    private static async Task SeedVendaAsync(HttpClient client, Guid produtoId, int quantidade, decimal valorPago)
    {
        var body = new
        {
            itens = new[] { new { produto_id = produtoId, quantidade } },
            valor_pago = valorPago
        };
        var response = await client.PostAsJsonAsync("/vendas", body, JsonOpts);
        response.EnsureSuccessStatusCode();
    }
}
