namespace Borgonha.Aplicacao.Lojas;

/// <summary>
/// Dados da loja: endereço, horários e canais de pedido.
/// </summary>
public sealed class ObterLoja(ILojaRepositorio repositorio)
{
    /// <returns>Os dados da loja, ou <see langword="null"/> se ela ainda não foi cadastrada.</returns>
    public async Task<LojaDto?> ExecutarAsync(CancellationToken cancellationToken)
    {
        var loja = await repositorio.ObterAsync(cancellationToken).ConfigureAwait(false);

        return loja?.ParaDto();
    }
}
