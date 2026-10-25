using Borgonha.Dominio.Comum;

namespace Borgonha.Dominio.Lojas;

/// <summary>
/// Pedido mínimo da entrega conforme a distância da loja: até o raio não há pedido mínimo;
/// mais longe, o pedido precisa chegar ao valor mínimo.
/// </summary>
public sealed record RegraDeEntrega
{
    public const int RaioMaximoEmMetros = 50_000;
    public const decimal PedidoMinimoMaximo = 9_999.99m;

    /// <summary>
    /// Folga em volta do raio. Os mapas gratuitos estimam o número da casa ao longo da rua, com erro de
    /// algumas centenas de metros: dentro dessa folga, a resposta seria um palpite.
    /// </summary>
    public const int MargemDeIncertezaEmMetros = 250;

    public RegraDeEntrega(int raioSemPedidoMinimoEmMetros, decimal pedidoMinimo)
    {
        if (raioSemPedidoMinimoEmMetros is <= 0 or > RaioMaximoEmMetros)
        {
            throw new DominioException("O raio sem pedido mínimo deve ser maior que zero e de no máximo 50 km.");
        }

        if (pedidoMinimo is <= 0 or > PedidoMinimoMaximo || decimal.Round(pedidoMinimo, 2) != pedidoMinimo)
        {
            throw new DominioException("O pedido mínimo deve ser maior que zero, até R$ 9.999,99 e com no máximo dois dígitos de centavos.");
        }

        RaioSemPedidoMinimoEmMetros = raioSemPedidoMinimoEmMetros;
        PedidoMinimo = pedidoMinimo;
    }

    public int RaioSemPedidoMinimoEmMetros { get; }

    public decimal PedidoMinimo { get; }

    /// <summary>
    /// Decide só pela distância em linha reta, quando ela já basta: o trajeto nunca é menor que a linha reta,
    /// então um endereço bem além do raio não precisa de rota.
    /// </summary>
    /// <returns>A situação, ou <see langword="null"/> se for preciso calcular o trajeto.</returns>
    public SituacaoDaEntrega? AvaliarPelaLinhaReta(int distanciaEmMetros)
    {
        return distanciaEmMetros > RaioSemPedidoMinimoEmMetros + MargemDeIncertezaEmMetros
            ? SituacaoDaEntrega.ComPedidoMinimo
            : null;
    }

    /// <summary>Decide pela distância do trajeto de entrega.</summary>
    public SituacaoDaEntrega AvaliarPeloTrajeto(int distanciaEmMetros)
    {
        if (Math.Abs(distanciaEmMetros - RaioSemPedidoMinimoEmMetros) <= MargemDeIncertezaEmMetros)
        {
            return SituacaoDaEntrega.Incerta;
        }

        return distanciaEmMetros < RaioSemPedidoMinimoEmMetros
            ? SituacaoDaEntrega.SemPedidoMinimo
            : SituacaoDaEntrega.ComPedidoMinimo;
    }
}
