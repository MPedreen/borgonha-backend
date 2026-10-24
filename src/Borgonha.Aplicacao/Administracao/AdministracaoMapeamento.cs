using Borgonha.Aplicacao.Catalogo;
using Borgonha.Dominio.Catalogo;
using Borgonha.Dominio.Comum;

namespace Borgonha.Aplicacao.Administracao;

internal static class AdministracaoMapeamento
{
    public static CategoriaAdminDto ParaDtoAdmin(this Categoria categoria) =>
        new(
            categoria.Slug.Valor,
            categoria.Nome,
            [.. categoria.Produtos.OrderBy(produto => produto.Ordem).Select(produto => produto.ParaDtoAdmin(categoria.Slug))]);

    public static ProdutoAdminDto ParaDtoAdmin(this Produto produto, Slug categoria) =>
        new(
            produto.Slug.Valor,
            produto.Nome,
            categoria.Valor,
            produto.Descricao,
            produto.Frase,
            produto.Peso,
            produto.Variacoes,
            produto.ImagensParaDto(),
            SituacoesDoProduto.ParaTexto(produto.Situacao));

    /// <summary>
    /// Texto alternativo das fotos enviadas pelo administrador (ex.: "Cookie Nutella partido ao meio").
    /// </summary>
    public static string TextoAlternativo(this Produto produto, TipoDeFoto tipo)
    {
        var nome = produto.Categoria.Slug.Valor == "cookies" && !produto.Nome.StartsWith("Cookie", StringComparison.OrdinalIgnoreCase)
            ? $"Cookie {produto.Nome}"
            : produto.Nome;

        return tipo == TipoDeFoto.Partido ? $"{nome} partido ao meio" : nome;
    }

    /// <summary>
    /// Depois de mudar o nome, as fotos passam a ser descritas pelo nome novo.
    /// </summary>
    public static void AtualizarTextosAlternativos(this Produto produto)
    {
        if (produto.ImagemInteiro is { } inteiro)
        {
            produto.TrocarImagemInteiro(new Imagem(inteiro.Caminho, produto.TextoAlternativo(TipoDeFoto.Inteiro)));
        }

        if (produto.ImagemPartido is { } partido)
        {
            produto.TrocarImagemPartido(new Imagem(partido.Caminho, produto.TextoAlternativo(TipoDeFoto.Partido)));
        }
    }
}
