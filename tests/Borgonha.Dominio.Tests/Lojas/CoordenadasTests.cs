using Borgonha.Dominio.Comum;
using Borgonha.Dominio.Lojas;

namespace Borgonha.Dominio.Tests.Lojas;

public sealed class CoordenadasTests
{
    [Fact]
    public void MedeADistanciaEmLinhaReta()
    {
        var loja = new Coordenadas(-22.8021494, -42.9803336);
        var umQuilometroAoSul = new Coordenadas(-22.8111, -42.9803336);

        Assert.InRange(loja.DistanciaEmLinhaRetaAte(umQuilometroAoSul), 995, 1_005);
        Assert.Equal(0, loja.DistanciaEmLinhaRetaAte(loja), 3);
    }

    [Theory]
    [InlineData(91, 0)]
    [InlineData(0, -181)]
    [InlineData(double.NaN, 0)]
    public void RecusaCoordenadasInvalidas(double latitude, double longitude)
    {
        Assert.Throws<DominioException>(() => new Coordenadas(latitude, longitude));
    }
}
