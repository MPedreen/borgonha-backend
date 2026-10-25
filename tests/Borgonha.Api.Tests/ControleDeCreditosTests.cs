using Borgonha.Infraestrutura.Mapas;
using Microsoft.Extensions.Options;

namespace Borgonha.Api.Tests;

public sealed class ControleDeCreditosTests
{
    private readonly RelogioDeTeste _relogio = new(new DateTimeOffset(2026, 10, 25, 12, 0, 0, TimeSpan.Zero));

    private ControleDeCreditos Criar(int creditosPorDia) =>
        new(Options.Create(new OpcoesDaGeoapify { CreditosPorDia = creditosPorDia }), _relogio);

    [Fact]
    public void LiberaAteOLimiteDoDiaEBarraDepois()
    {
        var controle = Criar(3);

        Assert.True(controle.TentarUsar());
        Assert.True(controle.TentarUsar());
        Assert.True(controle.TentarUsar());
        Assert.False(controle.TentarUsar());
        Assert.Equal(3, controle.Gastos);
    }

    [Fact]
    public void ORecomecoDeCreditosAcontecePassadaAMeiaNoiteUtc()
    {
        var controle = Criar(1);
        Assert.True(controle.TentarUsar());
        Assert.False(controle.TentarUsar());

        _relogio.Agora = new DateTimeOffset(2026, 10, 26, 0, 0, 1, TimeSpan.Zero);

        Assert.Equal(0, controle.Gastos);
        Assert.True(controle.TentarUsar());
    }

    [Fact]
    public void PausaBarraAsChamadasPorUmTempoMesmoComCredito()
    {
        var controle = Criar(100);

        controle.Pausar(TimeSpan.FromMinutes(1));
        Assert.False(controle.TentarUsar());

        _relogio.Agora += TimeSpan.FromSeconds(61);
        Assert.True(controle.TentarUsar());
    }
}

/// <summary>Relógio que só anda quando o teste manda.</summary>
internal sealed class RelogioDeTeste(DateTimeOffset inicio) : TimeProvider
{
    public DateTimeOffset Agora { get; set; } = inicio;

    public override DateTimeOffset GetUtcNow() => Agora;
}
