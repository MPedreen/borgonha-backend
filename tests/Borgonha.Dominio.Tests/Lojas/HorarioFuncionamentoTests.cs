using Borgonha.Dominio.Comum;
using Borgonha.Dominio.Lojas;

namespace Borgonha.Dominio.Tests.Lojas;

public sealed class HorarioFuncionamentoTests
{
    private static readonly DayOfWeek[] TercaASexta = [DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday];

    [Fact]
    public void ListaOsDiasDoIntervalo()
    {
        var horario = new HorarioFuncionamento(DayOfWeek.Tuesday, DayOfWeek.Friday, new TimeOnly(11, 0), new TimeOnly(19, 0));

        Assert.Equal(TercaASexta, horario.Dias());
    }

    [Fact]
    public void AceitaUmDiaSo()
    {
        var horario = new HorarioFuncionamento(DayOfWeek.Saturday, DayOfWeek.Saturday, new TimeOnly(11, 0), new TimeOnly(16, 0));

        Assert.Equal(DayOfWeek.Saturday, Assert.Single(horario.Dias()));
    }

    [Fact]
    public void RecusaFechamentoAntesDaAbertura()
    {
        Assert.Throws<DominioException>(() =>
            new HorarioFuncionamento(DayOfWeek.Tuesday, DayOfWeek.Friday, new TimeOnly(19, 0), new TimeOnly(11, 0)));
    }

    [Fact]
    public void RecusaDiaFinalAntesDoInicial()
    {
        Assert.Throws<DominioException>(() =>
            new HorarioFuncionamento(DayOfWeek.Friday, DayOfWeek.Tuesday, new TimeOnly(11, 0), new TimeOnly(19, 0)));
    }
}
