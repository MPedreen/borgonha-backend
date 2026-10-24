using Borgonha.Dominio.Comum;
using Borgonha.Dominio.Lojas;

namespace Borgonha.Dominio.Tests.Lojas;

public sealed class EnderecoTests
{
    [Fact]
    public void CriaEnderecoComCepOpcional()
    {
        var endereco = new Endereco("Rua Afrânio Peixoto", "40", "Santa Luzia", "São Gonçalo", "RJ");

        Assert.Equal("RJ", endereco.Uf);
        Assert.Null(endereco.Cep);
    }

    [Theory]
    [InlineData("rj")]
    [InlineData("R")]
    [InlineData("R1")]
    public void RecusaUfInvalida(string uf)
    {
        Assert.Throws<DominioException>(() => new Endereco("Rua Afrânio Peixoto", "40", "Santa Luzia", "São Gonçalo", uf));
    }

    [Fact]
    public void RecusaCepForaDoFormato()
    {
        Assert.Throws<DominioException>(() => new Endereco("Rua Afrânio Peixoto", "40", "Santa Luzia", "São Gonçalo", "RJ", "24400000"));
    }

    [Theory]
    [InlineData(-91, 0)]
    [InlineData(0, 181)]
    public void RecusaCoordenadasForaDoIntervalo(double latitude, double longitude)
    {
        Assert.Throws<DominioException>(() => new Coordenadas(latitude, longitude));
    }
}
