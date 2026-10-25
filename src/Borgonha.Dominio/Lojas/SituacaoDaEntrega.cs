namespace Borgonha.Dominio.Lojas;

public enum SituacaoDaEntrega
{
    /// <summary>Dentro do raio: entrega grátis, sem pedido mínimo.</summary>
    SemPedidoMinimo,

    /// <summary>Fora do raio: entrega grátis a partir do pedido mínimo.</summary>
    ComPedidoMinimo,

    /// <summary>Além de onde a consulta automática responde: confirmar com a loja.</summary>
    ForaDoAlcance,
}
