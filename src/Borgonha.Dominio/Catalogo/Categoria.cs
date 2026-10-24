using Borgonha.Dominio.Comum;

namespace Borgonha.Dominio.Catalogo;

/// <summary>
/// Grupo de produtos do cardápio (cookies, doces, salgados).
/// </summary>
public sealed class Categoria
{
    public const int TamanhoMaximoNome = 40;

    private readonly List<Produto> _produtos = [];

    private Categoria()
    {
    }

    public int Id { get; private set; }

    public Slug Slug { get; private set; } = null!;

    public string Nome { get; private set; } = string.Empty;

    public int Ordem { get; private set; }

    public IReadOnlyCollection<Produto> Produtos => _produtos.AsReadOnly();

    public static Categoria Criar(Slug slug, string nome, int ordem)
    {
        ArgumentNullException.ThrowIfNull(slug);

        return new Categoria
        {
            Slug = slug,
            Nome = Garantir.TextoObrigatorio(nome, "Nome da categoria", TamanhoMaximoNome),
            Ordem = Garantir.NaoNegativo(ordem, "Ordem da categoria"),
        };
    }

    public void AdicionarProduto(Produto produto)
    {
        ArgumentNullException.ThrowIfNull(produto);

        if (_produtos.Exists(existente => existente.Slug == produto.Slug))
        {
            throw new DominioException($"A categoria '{Nome}' já tem um produto com o slug '{produto.Slug}'.");
        }

        _produtos.Add(produto);
    }

    /// <summary>
    /// Posição de um produto novo: depois de todos os que já existem.
    /// </summary>
    public int ProximaOrdem() => _produtos.Count == 0 ? 0 : _produtos.Max(produto => produto.Ordem) + 1;
}
