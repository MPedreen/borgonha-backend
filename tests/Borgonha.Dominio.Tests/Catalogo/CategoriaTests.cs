using Borgonha.Dominio.Catalogo;
using Borgonha.Dominio.Comum;

namespace Borgonha.Dominio.Tests.Catalogo;

public sealed class CategoriaTests
{
    [Fact]
    public void AdicionaProdutos()
    {
        var categoria = Categoria.Criar(Slug.Criar("cookies"), "Cookies", 0);

        categoria.AdicionarProduto(Produto.Criar(Slug.Criar("nutella"), "Nutella", "Recheio de Nutella.", 0));

        Assert.Single(categoria.Produtos);
    }

    [Fact]
    public void RecusaProdutoComSlugRepetido()
    {
        var categoria = Categoria.Criar(Slug.Criar("cookies"), "Cookies", 0);
        categoria.AdicionarProduto(Produto.Criar(Slug.Criar("nutella"), "Nutella", "Recheio de Nutella.", 0));

        Assert.Throws<DominioException>(() =>
            categoria.AdicionarProduto(Produto.Criar(Slug.Criar("nutella"), "Outra Nutella", "Outra descrição.", 1)));
    }

    [Fact]
    public void RecusaOrdemNegativa()
    {
        Assert.Throws<DominioException>(() => Categoria.Criar(Slug.Criar("cookies"), "Cookies", -1));
    }
}
