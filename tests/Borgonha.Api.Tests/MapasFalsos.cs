using Borgonha.Aplicacao.Entrega;
using Borgonha.Dominio.Lojas;

namespace Borgonha.Api.Tests;

/// <summary>
/// Serviço de mapas dos testes: nenhuma chamada sai para a internet nem gasta crédito. O trajeto é a linha reta
/// mais 30%, e os endereços conhecidos são só os do dicionário.
/// </summary>
internal sealed class MapasFalsos : IGeocodificador, IRotas
{
    public const double FatorDoTrajeto = 1.3;

    private int _buscas;
    private int _trajetos;

    public bool Indisponivel { get; set; }

    public bool SemTrajeto { get; set; }

    public int Buscas => _buscas;

    public int Trajetos => _trajetos;

    public Task<IReadOnlyList<EnderecoSugeridoDto>> BuscarAsync(string texto, Coordenadas perto, CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _buscas);
        ChecarDisponibilidade();

        IReadOnlyList<EnderecoSugeridoDto> sugestoes = texto.Contains("afranio", StringComparison.OrdinalIgnoreCase)
            ? [new("Rua Afranio Peixoto 25", "Santa Luzia, São Gonçalo - RJ, 24725-270", -22.802102, -42.980427)]
            : [];

        return Task.FromResult(sugestoes);
    }

    public Task<int> DistanciaEmMetrosAsync(Coordenadas origem, Coordenadas destino, CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _trajetos);
        ChecarDisponibilidade();

        if (SemTrajeto)
        {
            throw new RotaNaoEncontradaException();
        }

        return Task.FromResult((int)Math.Round(origem.DistanciaEmLinhaRetaAte(destino) * FatorDoTrajeto));
    }

    public void Reiniciar()
    {
        Indisponivel = false;
        SemTrajeto = false;
        _buscas = 0;
        _trajetos = 0;
    }

    private void ChecarDisponibilidade()
    {
        if (Indisponivel)
        {
            throw new MapasIndisponiveisException();
        }
    }
}
