using Borgonha.Aplicacao.Catalogo;

namespace Borgonha.Aplicacao.Administracao;

/// <summary>
/// Cardápio completo para a área do administrador, inclusive os produtos ocultos.
/// </summary>
public sealed record CardapioAdminDto(IReadOnlyList<CategoriaAdminDto> Categorias);

public sealed record CategoriaAdminDto(string Slug, string Nome, IReadOnlyList<ProdutoAdminDto> Produtos);

/// <param name="Situacao">"no-cardapio", "esgotado" ou "oculto" (veja <see cref="SituacoesDoProduto"/>).</param>
public sealed record ProdutoAdminDto(
    string Slug,
    string Nome,
    string Categoria,
    string Descricao,
    string? Frase,
    string? Peso,
    IReadOnlyList<string> Variacoes,
    ImagensProdutoDto Imagens,
    string Situacao);

/// <summary>
/// Textos editáveis de um produto.
/// </summary>
public sealed record DadosDoProdutoDto(
    string Nome,
    string Descricao,
    string? Frase,
    string? Peso,
    IReadOnlyList<string>? Variacoes);

/// <param name="Categoria">Slug da categoria: "cookies", "doces" ou "salgados".</param>
public sealed record NovoProdutoDto(
    string Categoria,
    string Nome,
    string Descricao,
    string? Frase,
    string? Peso,
    IReadOnlyList<string>? Variacoes);

public sealed record SituacaoDoProdutoDto(string Situacao);
