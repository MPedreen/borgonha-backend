namespace Borgonha.Dominio.Catalogo;

/// <summary>
/// Como o produto aparece no site. O cardápio é semanal: o que está no site é o que tem no dia.
/// </summary>
public enum SituacaoProduto
{
    /// <summary>Aparece no site e pode ser pedido.</summary>
    NoCardapio,

    /// <summary>Aparece no site com o aviso "Esgotado hoje", sem o botão de pedido.</summary>
    Esgotado,

    /// <summary>Saiu do cardápio da semana e não aparece no site.</summary>
    Oculto,
}
