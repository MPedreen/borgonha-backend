using Borgonha.Dominio.Catalogo;
using Borgonha.Dominio.Comum;

namespace Borgonha.Dominio.Tests.Catalogo;

public sealed class ProdutoTests
{
    private static Produto CriarProduto(string nome = "Red Velvet", IEnumerable<string>? variacoes = null) =>
        Produto.Criar(Slug.Criar("red-velvet"), nome, "Massa red velvet com recheio de brigadeiro de Ninho.", 0, variacoes: variacoes);

    [Fact]
    public void CriaProdutoDisponivelERemoveEspacosDasPontas()
    {
        var produto = CriarProduto(nome: "  Red Velvet  ");

        Assert.Equal("Red Velvet", produto.Nome);
        Assert.True(produto.Disponivel);
        Assert.Null(produto.Preco);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ExigeNome(string nome)
    {
        Assert.Throws<DominioException>(() => CriarProduto(nome: nome));
    }

    [Fact]
    public void RecusaNomeMaiorQueOLimite()
    {
        Assert.Throws<DominioException>(() => CriarProduto(nome: new string('a', Produto.TamanhoMaximoNome + 1)));
    }

    [Fact]
    public void GuardaAsVariacoes()
    {
        var produto = CriarProduto(variacoes: ["Chocolate & Maracujá", " Nutella & Ninho "]);

        Assert.Equal(["Chocolate & Maracujá", "Nutella & Ninho"], produto.Variacoes);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RecusaPrecoMenorOuIgualAZero(int preco)
    {
        var produto = CriarProduto();

        Assert.Throws<DominioException>(() => produto.DefinirPreco(preco));
    }

    [Fact]
    public void DefinePreco()
    {
        var produto = CriarProduto();

        produto.DefinirPreco(12.90m);

        Assert.Equal(12.90m, produto.Preco);
    }

    [Fact]
    public void TiraOPreco()
    {
        var produto = CriarProduto();
        produto.DefinirPreco(15m);

        produto.DefinirPreco(null);

        Assert.Null(produto.Preco);
    }

    [Theory]
    [InlineData("12.999")]
    [InlineData("10000")]
    public void RecusaPrecoComMaisDeDoisCentavosOuAlemDoLimite(string preco)
    {
        var produto = CriarProduto();

        Assert.Throws<DominioException>(() => produto.DefinirPreco(decimal.Parse(preco, System.Globalization.CultureInfo.InvariantCulture)));
    }

    [Fact]
    public void AceitaProdutoSemDescricao()
    {
        var produto = Produto.Criar(Slug.Criar("matilda"), "Matilda", null, 0);

        produto.Atualizar("Matilda", "   ", null, null, null);

        Assert.Null(produto.Descricao);
    }

    [Fact]
    public void GuardaOPesoSemEspacosNasPontas()
    {
        var produto = Produto.Criar(Slug.Criar("tradicional"), "Tradicional", "Massa de manteiga tostada.", 0, peso: " 100g ");

        Assert.Equal("100g", produto.Peso);
    }

    [Fact]
    public void RecusaPesoMaiorQueOLimite()
    {
        Assert.Throws<DominioException>(() =>
            Produto.Criar(Slug.Criar("tradicional"), "Tradicional", "Massa de manteiga tostada.", 0, peso: new string('1', Produto.TamanhoMaximoPeso + 1)));
    }

    [Fact]
    public void AlternaEntreEsgotadoEDisponivel()
    {
        var produto = CriarProduto();

        produto.MarcarComoEsgotado();
        Assert.False(produto.Disponivel);

        produto.MarcarComoDisponivel();
        Assert.True(produto.Disponivel);
    }

    [Fact]
    public void ProdutoNovoEstaNoCardapio()
    {
        Assert.Equal(SituacaoProduto.NoCardapio, CriarProduto().Situacao);
    }

    [Theory]
    [InlineData(SituacaoProduto.NoCardapio, true, true)]
    [InlineData(SituacaoProduto.Esgotado, true, false)]
    [InlineData(SituacaoProduto.Oculto, false, true)]
    public void DefineASituacao(SituacaoProduto situacao, bool ativo, bool disponivel)
    {
        var produto = CriarProduto();
        produto.DefinirSituacao(SituacaoProduto.Esgotado);

        produto.DefinirSituacao(situacao);

        Assert.Equal(situacao, produto.Situacao);
        Assert.Equal(ativo, produto.Ativo);
        Assert.Equal(disponivel, produto.Disponivel);
    }

    [Fact]
    public void RecusaSituacaoInexistente()
    {
        Assert.Throws<DominioException>(() => CriarProduto().DefinirSituacao((SituacaoProduto)42));
    }

    [Fact]
    public void AtualizaOsTextosSemMudarOSlug()
    {
        var produto = CriarProduto();

        produto.Atualizar(" Pistache ", "Recheio de pistache.", null, "130g", ["", " Inteiro "]);

        Assert.Equal("red-velvet", produto.Slug.Valor);
        Assert.Equal("Pistache", produto.Nome);
        Assert.Equal("Recheio de pistache.", produto.Descricao);
        Assert.Null(produto.Frase);
        Assert.Equal("130g", produto.Peso);
        Assert.Equal(["Inteiro"], produto.Variacoes);
    }

    [Fact]
    public void ValidaOsTextosAoAtualizar()
    {
        Assert.Throws<DominioException>(() => CriarProduto().Atualizar(" ", "Recheio de Galak.", null, null, null));
    }

    [Fact]
    public void TrocaERemoveAsFotos()
    {
        var produto = CriarProduto();

        produto.TrocarImagemInteiro(new Imagem("envios/abc", "Cookie Red Velvet"));
        produto.TrocarImagemPartido(new Imagem("envios/def", "Cookie Red Velvet partido ao meio"));
        produto.TrocarImagemPartido(null);

        Assert.Equal("envios/abc", produto.ImagemInteiro?.Caminho);
        Assert.Null(produto.ImagemPartido);
    }
}
