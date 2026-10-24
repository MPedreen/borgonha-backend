namespace Borgonha.Api.Configuracao;

internal static class PoliticasDeCache
{
    /// <summary>
    /// Conteúdo do site (cardápio, loja e dúvidas): muda pouco, então fica 5 minutos em cache.
    /// </summary>
    public const string Conteudo = "conteudo";
}
