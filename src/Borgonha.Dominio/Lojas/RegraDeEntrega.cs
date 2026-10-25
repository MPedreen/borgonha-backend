using Borgonha.Dominio.Comum;

namespace Borgonha.Dominio.Lojas;

/// <summary>
/// Pedido mínimo da entrega conforme a distância da loja: até o raio (em linha reta) não há pedido mínimo;
/// mais longe, o pedido precisa chegar ao valor mínimo.
/// </summary>
public sealed record RegraDeEntrega
{
    public const int RaioMaximoEmMetros = 50_000;
    public const decimal PedidoMinimoMaximo = 9_999.99m;

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
}
