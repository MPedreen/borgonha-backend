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

    /// <summary>
    /// Falso quando o produto esgotou: continua no site, com o aviso "Esgotado hoje".
    /// </summary>
    public bool Disponivel { get; private set; } = true;

    /// <summary>
    /// Falso quando o produto saiu do cardápio da semana: some do site até voltar.
    /// </summary>
    public bool Ativo { get; private set; } = true;

    public SituacaoProduto Situacao =>
        !Ativo ? SituacaoProduto.Oculto
        : Disponivel ? SituacaoProduto.NoCardapio
        : SituacaoProduto.Esgotado;

    public int Ordem { get; private set; }

    public static Produto Criar(
        Slug slug,
        string nome,
        string descricao,
        int ordem,
        string? frase = null,
        string? peso = null,
        IEnumerable<string>? variacoes = null,
        Imagem? imagemInteiro = null,
        Imagem? imagemPartido = null)
    {
        ArgumentNullException.ThrowIfNull(slug);

        var produto = new Produto
        {
            Slug = slug,
            ImagemInteiro = imagemInteiro,
            ImagemPartido = imagemPartido,
            Ordem = Garantir.NaoNegativo(ordem, "Ordem do produto"),
        };
        produto.Atualizar(nome, descricao, frase, peso, variacoes);

        return produto;
    }

    /// <summary>
    /// Troca os textos do produto (o slug continua o mesmo, para não quebrar links e âncoras).
    /// </summary>
    public void Atualizar(string nome, string descricao, string? frase, string? peso, IEnumerable<string>? variacoes)
    {
        Nome = Garantir.TextoObrigatorio(nome, "Nome do produto", TamanhoMaximoNome);
        Descricao = Garantir.TextoObrigatorio(descricao, "Descrição do produto", TamanhoMaximoDescricao);
        Frase = Garantir.TextoOpcional(frase, "Frase do produto", TamanhoMaximoFrase);
        Peso = Garantir.TextoOpcional(peso, "Peso do produto", TamanhoMaximoPeso);
        Variacoes = [.. (variacoes ?? [])
            .Where(variacao => !string.IsNullOrWhiteSpace(variacao))
            .Select(variacao => Garantir.TextoObrigatorio(variacao, "Variação do produto", TamanhoMaximoVariacao))];
    }

    public void TrocarImagemInteiro(Imagem? imagem) => ImagemInteiro = imagem;

    public void TrocarImagemPartido(Imagem? imagem) => ImagemPartido = imagem;

    public void DefinirSituacao(SituacaoProduto situacao)
    {
        if (!Enum.IsDefined(situacao))
        {
            throw new DominioException("Situação do produto inválida.");
        }

        Ativo = situacao != SituacaoProduto.Oculto;
        Disponivel = situacao != SituacaoProduto.Esgotado;
    }

    public void DefinirPreco(decimal? preco)
    {
        if (preco is <= 0)
        {
            throw new DominioException("O preço deve ser maior que zero.");
        }

        Preco = preco;
    }

    public void DefinirSelo(string? selo) =>
        Selo = Garantir.TextoOpcional(selo, "Selo do produto", TamanhoMaximoSelo);

    public void MarcarComoEsgotado() => Disponivel = false;

    public void MarcarComoDisponivel() => Disponivel = true;
}
