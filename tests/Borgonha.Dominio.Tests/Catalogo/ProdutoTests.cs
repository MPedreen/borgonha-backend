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

        Assert.Throws<DominioException>(() => produto.DefinirPreco(preco, "100 g"));
    }

    [Fact]
    public void DefinePrecoEPeso()
    {
        var produto = CriarProduto();

        produto.DefinirPreco(12.90m, "100 g");

        Assert.Equal(12.90m, produto.Preco);
        Assert.Equal("100 g", produto.Peso);
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
}
