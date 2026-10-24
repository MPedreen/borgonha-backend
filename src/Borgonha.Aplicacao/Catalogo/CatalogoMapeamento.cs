using Borgonha.Dominio.Catalogo;
using Borgonha.Dominio.Comum;

namespace Borgonha.Aplicacao.Catalogo;

internal static class CatalogoMapeamento
{
    public static CategoriaDto ParaDto(this Categoria categoria) =>
        new(
            categoria.Slug.Valor,
            categoria.Nome,
            [.. categoria.Produtos.OrderBy(produto => produto.Ordem).Select(produto => produto.ParaDto(categoria.Slug))]);

    public static ProdutoDto ParaDto(this Produto produto, Slug categoria) =>
        new(
            produto.Slug.Valor,
            produto.Nome,
            categoria.Valor,
            produto.Descricao,
            produto.Frase,
            produto.Preco,
            produto.Peso,
            produto.Selo,
            produto.Variacoes,
            new ImagensProdutoDto(produto.ImagemInteiro?.ParaDto(), produto.ImagemPartido?.ParaDto()),
            produto.Disponivel);

    private static ImagemDto ParaDto(this Imagem imagem) => new(imagem.Caminho, imagem.TextoAlternativo);
}
