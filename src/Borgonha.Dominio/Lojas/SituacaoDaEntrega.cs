namespace Borgonha.Dominio.Lojas;

public enum SituacaoDaEntrega
{
    /// <summary>Dentro do raio: sem pedido mínimo (a entrega é sempre grátis).</summary>
    SemPedidoMinimo,

    /// <summary>Fora do raio, a qualquer distância: pedido mínimo obrigatório.</summary>
    ComPedidoMinimo,

    /// <summary>Perto demais do limite do raio para o mapa decidir: confirmar com a loja.</summary>
    Incerta,
}
