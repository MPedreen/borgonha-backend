using Borgonha.Dominio.Comum;
using Borgonha.Dominio.Lojas;

namespace Borgonha.Dominio.Tests.Lojas;

public sealed class LojaTests
{
    private static readonly Uri LinkWhatsApp = new("https://wa.me/5521974509789");

    private static Loja CriarLoja(string telefone = "+5521974509789", string? instagram = "borgonhaconfeitaria") =>
        Loja.Criar(
            "Borgonha Confeitaria",
            new Endereco("Rua Afrânio Peixoto", "40", "Santa Luzia", "São Gonçalo", "RJ"),
            new Coordenadas(-22.8021494, -42.9803336),
            telefone,
            instagram);

    [Theory]
    [InlineData("21974509789")]
    [InlineData("+55 21 97450-9789")]
    [InlineData("+0521974509789")]
    public void ExigeTelefoneNoFormatoInternacional(string telefone)
    {
        Assert.Throws<DominioException>(() => CriarLoja(telefone: telefone));
    }

    [Fact]
    public void RecusaInstagramComArroba()
    {
        Assert.Throws<DominioException>(() => CriarLoja(instagram: "@borgonhaconfeitaria"));
    }

    [Fact]
    public void AceitaLojaSemInstagram()
    {
        Assert.Null(CriarLoja(instagram: null).Instagram);
    }

    [Fact]
    public void RecusaHorariosComDiasRepetidos()
    {
        var loja = CriarLoja();
        loja.AdicionarHorario(new HorarioFuncionamento(DayOfWeek.Tuesday, DayOfWeek.Friday, new TimeOnly(11, 0), new TimeOnly(19, 0)));

        Assert.Throws<DominioException>(() =>
            loja.AdicionarHorario(new HorarioFuncionamento(DayOfWeek.Friday, DayOfWeek.Saturday, new TimeOnly(11, 0), new TimeOnly(16, 0))));
    }

    [Fact]
    public void RecusaDoisCanaisDoMesmoTipo()
    {
        var loja = CriarLoja();
        loja.AdicionarCanalPedido(new CanalPedido(TipoCanalPedido.WhatsApp, "WhatsApp", "Melhores preços.", LinkWhatsApp, 0));

        Assert.Throws<DominioException>(() =>
            loja.AdicionarCanalPedido(new CanalPedido(TipoCanalPedido.WhatsApp, "Outro WhatsApp", "Outro texto.", LinkWhatsApp, 1)));
    }
}
