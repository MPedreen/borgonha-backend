using System.Net.Http.Json;
using Borgonha.Aplicacao.Lojas;

namespace Borgonha.Api.Tests;

public sealed class LojaEndpointsTests(BorgonhaApiFactory fabrica) : IClassFixture<BorgonhaApiFactory>
{
    private static readonly string[] CanaisEsperados = ["whatsapp", "ifood", "loja-fisica"];
    private static readonly string[] TercaASexta = ["tuesday", "wednesday", "thursday", "friday"];
    private static readonly string[] Sabado = ["saturday"];

    private readonly HttpClient _cliente = fabrica.CreateClient();

    [Fact]
    public async Task RetornaOsCanaisDePedidoComOWhatsAppEmDestaque()
    {
        var loja = await ObterLojaAsync();

        Assert.Equal(CanaisEsperados, loja.CanaisPedido.Select(canal => canal.Tipo));

        var whatsApp = loja.CanaisPedido[0];
        Assert.True(whatsApp.Destaque);
        Assert.Equal(new Uri("https://wa.me/5521974509789"), whatsApp.Url);
        Assert.Equal("Olá!\nDesejo fazer meu pedido!", whatsApp.MensagemPadrao);
        Assert.Equal("MELHORES PREÇOS, entrega exclusiva em Santa Luzia e região com frete grátis!", whatsApp.Descricao);
    }

    [Fact]
    public async Task RetornaEnderecoHorariosEInstagram()
    {
        var loja = await ObterLojaAsync();

        Assert.Equal("Rua Afrânio Peixoto", loja.Endereco.Logradouro);
        Assert.Equal("40", loja.Endereco.Numero);
        Assert.Equal("borgonhaconfeitaria", loja.Instagram);

        Assert.Collection(
            loja.Horarios,
            semana =>
            {
                Assert.Equal(TercaASexta, semana.Dias);
                Assert.Equal("11:00", semana.Abertura);
                Assert.Equal("19:00", semana.Fechamento);
            },
            sabado =>
            {
                Assert.Equal(Sabado, sabado.Dias);
                Assert.Equal("11:00", sabado.Abertura);
                Assert.Equal("16:00", sabado.Fechamento);
            });
    }

    [Fact]
    public async Task RetornaARegraDoPedidoMinimoDaEntrega()
    {
        var loja = await ObterLojaAsync();

        Assert.NotNull(loja.Entrega);
        Assert.Equal(1_500, loja.Entrega.RaioSemPedidoMinimoEmMetros);
        Assert.Equal(30m, loja.Entrega.PedidoMinimo);
    }

    private async Task<LojaDto> ObterLojaAsync()
    {
        var loja = await _cliente.GetFromJsonAsync<LojaDto>("/api/loja", TestContext.Current.CancellationToken);

        Assert.NotNull(loja);
        return loja;
    }
}
