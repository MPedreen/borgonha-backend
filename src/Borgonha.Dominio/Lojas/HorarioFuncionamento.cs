using Borgonha.Dominio.Comum;

namespace Borgonha.Dominio.Lojas;

/// <summary>
/// Horário de um intervalo de dias da semana (ex.: terça a sexta, das 11h às 19h).
/// </summary>
public sealed class HorarioFuncionamento
{
    public HorarioFuncionamento(DayOfWeek diaInicial, DayOfWeek diaFinal, TimeOnly abertura, TimeOnly fechamento)
    {
        if (!Enum.IsDefined(diaInicial) || !Enum.IsDefined(diaFinal))
        {
            throw new DominioException("Dia da semana inválido.");
        }

        if (diaFinal < diaInicial)
        {
            throw new DominioException("O dia final deve ser igual ou posterior ao dia inicial (domingo é o primeiro dia da semana).");
        }

        if (fechamento <= abertura)
        {
            throw new DominioException("O horário de fechamento deve ser posterior ao de abertura.");
        }

        DiaInicial = diaInicial;
        DiaFinal = diaFinal;
        Abertura = abertura;
        Fechamento = fechamento;
    }

    public DayOfWeek DiaInicial { get; private set; }

    public DayOfWeek DiaFinal { get; private set; }

    public TimeOnly Abertura { get; private set; }

    public TimeOnly Fechamento { get; private set; }

    public IEnumerable<DayOfWeek> Dias() =>
        Enumerable.Range((int)DiaInicial, DiaFinal - DiaInicial + 1).Select(dia => (DayOfWeek)dia);

    internal bool SobrepoeA(HorarioFuncionamento outro) => Dias().Intersect(outro.Dias()).Any();
}
