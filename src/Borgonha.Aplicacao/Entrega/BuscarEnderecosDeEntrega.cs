using System.Text.RegularExpressions;
using Borgonha.Aplicacao.Lojas;
using Borgonha.Dominio.Comum;

namespace Borgonha.Aplicacao.Entrega;

/// <summary>
/// Sugestões de endereço para o cliente conferir se a entrega tem pedido mínimo.
/// </summary>
public sealed partial class BuscarEnderecosDeEntrega(ILojaRepositorio lojas, IGeocodificador geocodificador)
{
    public const int TamanhoMinimo = 4;
    public const int TamanhoMaximo = 120;

    /// <exception cref="DominioException">O texto é curto ou longo demais.</exception>
    /// <exception cref="MapasIndisponiveisException">O serviço de mapas não pôde responder agora.</exception>
    public async Task<IReadOnlyList<EnderecoSugeridoDto>> ExecutarAsync(string? texto, CancellationToken cancellationToken)
    {
        var limpo = Espacos().Replace(texto ?? string.Empty, " ").Trim();

        if (limpo.Length < TamanhoMinimo)
        {
            throw new DominioException($"Digite pelo menos {TamanhoMinimo} letras do endereço.");
        }

        if (limpo.Length > TamanhoMaximo)
        {
            throw new DominioException($"O endereço deve ter no máximo {TamanhoMaximo} caracteres.");
        }

        var loja = await lojas.ObterAsync(cancellationToken).ConfigureAwait(false)
            ?? throw new DominioException("A loja ainda não foi cadastrada.");

        return await geocodificador.BuscarAsync(limpo, loja.Coordenadas, cancellationToken).ConfigureAwait(false);
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex Espacos();
}
