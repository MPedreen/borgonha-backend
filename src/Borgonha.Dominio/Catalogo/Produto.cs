using Borgonha.Dominio.Comum;

namespace Borgonha.Dominio.Catalogo;

/// <summary>
/// Item do cardápio exibido no site.
/// </summary>
public sealed class Produto
{
    public const int TamanhoMaximoNome = 80;
    public const int TamanhoMaximoDescricao = 400;
    public const int TamanhoMaximoFrase = 120;
    public const int TamanhoMaximoPeso = 20;
    public const int TamanhoMaximoSelo = 24;
    public const int TamanhoMaximoVariacao = 60;

    private Produto()
    {
    }

    public int Id { get; private set; }

    public int CategoriaId { get; private set; }

    public Categoria Categoria { get; private set; } = null!;

    public Slug Slug { get; private set; } = null!;

    public string Nome { get; private set; } = string.Empty;

    public string Descricao { get; private set; } = string.Empty;

    /// <summary>
    /// Frase curta e divertida usada no menu de sabores.
    /// </summary>
    public string? Frase { get; private set; }

    public decimal? Preco { get; private set; }

    /// <summary>
    /// Peso ou porção, em texto livre (ex.: "100 g", "cento").
    /// </summary>
    public string? Peso { get; private set; }

    /// <summary>
    /// Destaque opcional exibido no card (ex.: "MAIS PEDIDO").
    /// </summary>
    public string? Selo { get; private set; }

    /// <summary>
    /// Sabores ou versões do mesmo produto (ex.: os sabores do bolo de pote).
    /// </summary>
    public IReadOnlyList<string> Variacoes { get; private set; } = [];

    /// <summary>
    /// Foto do produto inteiro (no caso dos cookies, visto de cima).
    /// </summary>
    public Imagem? ImagemInteiro { get; private set; }

    /// <summary>
    /// Foto do produto partido ao meio, mostrando o recheio.
    /// </summary>
    public Imagem? ImagemPartido { get; private set; }

    public bool Disponivel { get; private set; } = true;

    public int Ordem { get; private set; }

    public static Produto Criar(
        Slug slug,
        string nome,
        string descricao,
        int ordem,
        string? frase = null,
        IEnumerable<string>? variacoes = null,
        Imagem? imagemInteiro = null,
        Imagem? imagemPartido = null)
    {
        ArgumentNullException.ThrowIfNull(slug);

        return new Produto
        {
            Slug = slug,
            Nome = Garantir.TextoObrigatorio(nome, "Nome do produto", TamanhoMaximoNome),
            Descricao = Garantir.TextoObrigatorio(descricao, "Descrição do produto", TamanhoMaximoDescricao),
            Frase = Garantir.TextoOpcional(frase, "Frase do produto", TamanhoMaximoFrase),
            Variacoes = [.. (variacoes ?? []).Select(variacao => Garantir.TextoObrigatorio(variacao, "Variação do produto", TamanhoMaximoVariacao))],
            ImagemInteiro = imagemInteiro,
            ImagemPartido = imagemPartido,
            Ordem = Garantir.NaoNegativo(ordem, "Ordem do produto"),
        };
    }

    public void DefinirPreco(decimal? preco, string? peso)
    {
        if (preco is <= 0)
        {
            throw new DominioException("O preço deve ser maior que zero.");
        }

        Preco = preco;
        Peso = Garantir.TextoOpcional(peso, "Peso do produto", TamanhoMaximoPeso);
    }

    public void DefinirSelo(string? selo) =>
        Selo = Garantir.TextoOpcional(selo, "Selo do produto", TamanhoMaximoSelo);

    public void MarcarComoEsgotado() => Disponivel = false;

    public void MarcarComoDisponivel() => Disponivel = true;
}
