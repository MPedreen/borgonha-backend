using Borgonha.Dominio.Lojas;

namespace Borgonha.Aplicacao.Entrega;

/// <summary>
/// Calcula a distância do trajeto de entrega pelas ruas.
/// </summary>
public interface IRotas
{
    /// <returns>A distância do trajeto, em metros.</returns>
    /// <exception cref="RotaNaoEncontradaException">Não há trajeto entre os dois pontos.</exception>
    /// <exception cref="MapasIndisponiveisException">O serviço de mapas não pôde responder agora.</exception>
    Task<int> DistanciaEmMetrosAsync(Coordenadas origem, Coordenadas destino, CancellationToken cancellationToken);
}
