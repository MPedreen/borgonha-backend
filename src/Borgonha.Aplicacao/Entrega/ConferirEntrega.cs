using Borgonha.Aplicacao.Lojas;
using Borgonha.Dominio.Comum;
using Borgonha.Dominio.Lojas;

namespace Borgonha.Aplicacao.Entrega;

/// <summary>
/// Diz se a entrega num ponto exige pedido mínimo. O trajeto só é calculado quando a linha reta não basta
/// para decidir, o que poupa o limite diário do serviço de mapas.
/// </summary>
public sealed class ConferirEntrega(ILojaRepositorio lojas, IRotas rotas)
{
    /// <exception cref="DominioException">Coordenadas inválidas ou loja sem regra de entrega.</exception>
    /// <exception cref="MapasIndisponiveisException">O serviço de mapas não pôde responder agora.</exception>
    public async Task<ResultadoDaEntregaDto> ExecutarAsync(double latitude, double longitude, CancellationToken cancellationToken)
    {
        var destino = new Coordenadas(latitude, longitude);
        var loja = await lojas.ObterAsync(cancellationToken).ConfigureAwait(false)
            ?? throw new DominioException("A loja ainda não foi cadastrada.");
        var regra = loja.Entrega ?? throw new DominioException("A loja não tem regra de entrega cadastrada.");

        var linhaReta = (int)Math.Round(loja.Coordenadas.DistanciaEmLinhaRetaAte(destino));

        if (regra.AvaliarPelaLinhaReta(linhaReta) is { } situacao)
        {
            return new ResultadoDaEntregaDto(Texto(situacao), null, linhaReta);
        }

        try
        {
            var trajeto = await rotas.DistanciaEmMetrosAsync(loja.Coordenadas, destino, cancellationToken).ConfigureAwait(false);

            return new ResultadoDaEntregaDto(Texto(regra.AvaliarPeloTrajeto(trajeto)), trajeto, linhaReta);
        }
        catch (RotaNaoEncontradaException)
        {
            // Sem trajeto, vale a linha reta, que aqui está dentro do raio com a tolerância.
            return new ResultadoDaEntregaDto(Texto(SituacaoDaEntrega.SemPedidoMinimo), null, linhaReta);
        }
    }

    private static string Texto(SituacaoDaEntrega situacao) =>
        situacao switch
        {
            SituacaoDaEntrega.SemPedidoMinimo => "sem-minimo",
            SituacaoDaEntrega.ComPedidoMinimo => "com-minimo",
            SituacaoDaEntrega.ForaDoAlcance => "fora-da-regiao",
            _ => throw new ArgumentOutOfRangeException(nameof(situacao), situacao, "Situação sem representação na API."),
        };
}
