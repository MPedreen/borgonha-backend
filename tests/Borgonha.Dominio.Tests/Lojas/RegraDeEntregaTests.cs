using Borgonha.Dominio.Comum;
using Borgonha.Dominio.Lojas;

namespace Borgonha.Dominio.Tests.Lojas;

public sealed class RegraDeEntregaTests
{
    [Fact]
    public void GuardaORaioEOPedidoMinimo()
    {
        var regra = new RegraDeEntrega(1_500, 30m);

        Assert.Equal(1_500, regra.RaioSemPedidoMinimoEmMetros);
        Assert.Equal(30m, regra.PedidoMinimo);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(50_001)]
    public void RecusaRaioForaDoLimite(int raio)
    {
        Assert.Throws<DominioException>(() => new RegraDeEntrega(raio, 30m));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-30")]
    [InlineData("30.555")]
    [InlineData("10000")]
    public void RecusaPedidoMinimoInvalido(string valor)
    {
        var pedidoMinimo = decimal.Parse(valor, System.Globalization.CultureInfo.InvariantCulture);

        Assert.Throws<DominioException>(() => new RegraDeEntrega(1_500, pedidoMinimo));
    }
}
