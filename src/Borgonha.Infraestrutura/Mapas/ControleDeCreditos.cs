using Microsoft.Extensions.Options;

namespace Borgonha.Infraestrutura.Mapas;

/// <summary>
/// Conta os créditos gastos no dia e barra as chamadas ao serviço de mapas antes de o plano gratuito acabar.
/// O dia segue o UTC e a contagem fica na memória: se a API reiniciar, ela recomeça do zero, e a pausa
/// por recusa do serviço (<see cref="Pausar"/>) é a segunda linha de defesa.
/// </summary>
internal sealed class ControleDeCreditos(IOptions<OpcoesDaGeoapify> opcoes, TimeProvider relogio)
{
    private readonly Lock _trava = new();
    private DateOnly _dia;
    private int _gastos;
    private DateTimeOffset _pausadoAte = DateTimeOffset.MinValue;

    /// <returns><see langword="true"/> se há crédito para a chamada (e ele já foi descontado).</returns>
    public bool TentarUsar(int creditos = 1)
    {
        lock (_trava)
        {
            var agora = relogio.GetUtcNow();
            var hoje = DateOnly.FromDateTime(agora.UtcDateTime);

            if (hoje != _dia)
            {
                _dia = hoje;
                _gastos = 0;
            }

            if (agora < _pausadoAte || _gastos + creditos > opcoes.Value.CreditosPorDia)
            {
                return false;
            }

            _gastos += creditos;
            return true;
        }
    }

    /// <summary>Suspende as chamadas por um tempo, quando o serviço recusa por excesso de uso.</summary>
    public void Pausar(TimeSpan duracao)
    {
        lock (_trava)
        {
            _pausadoAte = relogio.GetUtcNow() + duracao;
        }
    }

    public int Gastos
    {
        get
        {
            lock (_trava)
            {
                return DateOnly.FromDateTime(relogio.GetUtcNow().UtcDateTime) == _dia ? _gastos : 0;
            }
        }
    }
}
