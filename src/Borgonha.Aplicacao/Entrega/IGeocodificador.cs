using Borgonha.Dominio.Lojas;

namespace Borgonha.Aplicacao.Entrega;

/// <summary>
/// Transforma o endereço que o cliente digita em sugestões com coordenadas.
/// </summary>
public interface IGeocodificador
{
    /// <param name="texto">Endereço digitado, já limpo e dentro dos limites de tamanho.</param>
    /// <param name="perto">Ponto usado para pôr os resultados mais próximos antes (a loja).</param>
    /// <exception cref="MapasIndisponiveisException">O serviço de mapas não pôde responder agora.</exception>
    Task<IReadOnlyList<EnderecoSugeridoDto>> BuscarAsync(string texto, Coordenadas perto, CancellationToken cancellationToken);
}
