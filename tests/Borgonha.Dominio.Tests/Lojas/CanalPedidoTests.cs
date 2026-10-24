using Borgonha.Dominio.Comum;
using Borgonha.Dominio.Lojas;

namespace Borgonha.Dominio.Tests.Lojas;

public sealed class CanalPedidoTests
{
    [Fact]
    public void GuardaAMensagemPadraoDoWhatsApp()
    {
        var canal = new CanalPedido(
            TipoCanalPedido.WhatsApp,
            "Pedir pelo WhatsApp",
            "Melhores preços.",
            new Uri("https://wa.me/5521974509789"),
            0,
            mensagemPadrao: "Olá!\nDesejo fazer meu pedido!",
            destaque: true);

        Assert.Equal("Olá!\nDesejo fazer meu pedido!", canal.MensagemPadrao);
        Assert.True(canal.Destaque);
    }

    [Fact]
    public void RecusaLinkSemHttps()
    {
        Assert.Throws<DominioException>(() =>
            new CanalPedido(TipoCanalPedido.IFood, "iFood", "Peça pelo iFood.", new Uri("http://www.ifood.com.br/"), 1));
    }

    [Fact]
    public void RecusaMensagemPadraoForaDoWhatsApp()
    {
        Assert.Throws<DominioException>(() =>
            new CanalPedido(TipoCanalPedido.IFood, "iFood", "Peça pelo iFood.", new Uri("https://www.ifood.com.br/"), 1, mensagemPadrao: "Olá!"));
    }
}
