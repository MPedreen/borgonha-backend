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

    [Theory]
    [InlineData(1_000, null)]
    [InlineData(1_750, null)]
    [InlineData(1_751, SituacaoDaEntrega.ComPedidoMinimo)]
    [InlineData(5_000, SituacaoDaEntrega.ComPedidoMinimo)]
    [InlineData(40_000, SituacaoDaEntrega.ComPedidoMinimo)]
    public void LinhaRetaDecideSoQuandoJaBasta(int distancia, SituacaoDaEntrega? esperada)
    {
        var regra = new RegraDeEntrega(1_500, 30m);

        Assert.Equal(esperada, regra.AvaliarPelaLinhaReta(distancia));
    }

    [Theory]
    [InlineData(900, SituacaoDaEntrega.SemPedidoMinimo)]
    [InlineData(1_249, SituacaoDaEntrega.SemPedidoMinimo)]
    [InlineData(1_250, SituacaoDaEntrega.Incerta)]
    [InlineData(1_500, SituacaoDaEntrega.Incerta)]
    [InlineData(1_750, SituacaoDaEntrega.Incerta)]
    [InlineData(1_751, SituacaoDaEntrega.ComPedidoMinimo)]
    public void TrajetoPertoDoLimiteFicaIncerto(int distancia, SituacaoDaEntrega esperada)
    {
        var regra = new RegraDeEntrega(1_500, 30m);

        Assert.Equal(esperada, regra.AvaliarPeloTrajeto(distancia));
    }
}
