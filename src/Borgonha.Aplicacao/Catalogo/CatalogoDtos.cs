namespace Borgonha.Aplicacao.Catalogo;

public sealed record CatalogoDto(IReadOnlyList<CategoriaDto> Categorias);

public sealed record CategoriaDto(string Slug, string Nome, IReadOnlyList<ProdutoDto> Produtos);

public sealed record ProdutoDto(
    string Slug,
    string Nome,
    string Categoria,
    string? Descricao,
    string? Frase,
    decimal? Preco,
    string? Peso,
    string? Selo,
    IReadOnlyList<string> Variacoes,
    ImagensProdutoDto Imagens,
    bool Disponivel);

public sealed record ImagensProdutoDto(ImagemDto? Inteiro, ImagemDto? Partido);

public sealed record ImagemDto(string Caminho, string Alt);
