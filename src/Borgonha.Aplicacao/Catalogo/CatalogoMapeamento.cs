using Borgonha.Dominio.Catalogo;
using Borgonha.Dominio.Comum;

namespace Borgonha.Aplicacao.Catalogo;

internal static class CatalogoMapeamento
{
    /// <summary>
    /// Categoria como o site mostra: só os produtos do cardápio da semana (os ocultos ficam de fora).
    /// </summary>
    public static CategoriaDto ParaDto(this Categoria categoria) =>
        new(
            categoria.Slug.Valor,
            categoria.Nome,
            [.. categoria.Produtos
                .Where(produto => produto.Ativo)
                .OrderBy(produto => produto.Ordem)
                .Select(produto => produto.ParaDto(categoria.Slug))]);

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
            produto.ImagensParaDto(),
            produto.Disponivel);

    public static ImagensProdutoDto ImagensParaDto(this Produto produto) =>
        new(produto.ImagemInteiro?.ParaDto(), produto.ImagemPartido?.ParaDto());

    private static ImagemDto ParaDto(this Imagem imagem) => new(imagem.Caminho, imagem.TextoAlternativo);
}
