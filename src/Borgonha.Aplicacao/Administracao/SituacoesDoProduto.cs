using Borgonha.Dominio.Catalogo;
using Borgonha.Dominio.Comum;

namespace Borgonha.Aplicacao.Administracao;

/// <summary>
/// Situação do produto como texto, no formato usado pela API.
/// </summary>
public static class SituacoesDoProduto
{
    public const string NoCardapio = "no-cardapio";
    public const string Esgotado = "esgotado";
    public const string Oculto = "oculto";

    public static string ParaTexto(SituacaoProduto situacao) => situacao switch
    {
        SituacaoProduto.NoCardapio => NoCardapio,
        SituacaoProduto.Esgotado => Esgotado,
        SituacaoProduto.Oculto => Oculto,
        _ => throw new ArgumentOutOfRangeException(nameof(situacao), situacao, null),
    };

    public static SituacaoProduto Ler(string? texto) => texto switch
    {
        NoCardapio => SituacaoProduto.NoCardapio,
        Esgotado => SituacaoProduto.Esgotado,
        Oculto => SituacaoProduto.Oculto,
        _ => throw new DominioException($"Situação inválida. Use \"{NoCardapio}\", \"{Esgotado}\" ou \"{Oculto}\"."),
    };
}
