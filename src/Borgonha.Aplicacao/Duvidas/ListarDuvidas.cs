namespace Borgonha.Aplicacao.Duvidas;

/// <summary>
/// Perguntas frequentes, na ordem de exibição.
/// </summary>
public sealed class ListarDuvidas(IDuvidaRepositorio repositorio)
{
    public async Task<IReadOnlyList<DuvidaDto>> ExecutarAsync(CancellationToken cancellationToken)
    {
        var duvidas = await repositorio.ListarAsync(cancellationToken).ConfigureAwait(false);

        return [.. duvidas.OrderBy(duvida => duvida.Ordem).Select(duvida => new DuvidaDto(duvida.Pergunta, duvida.Resposta))];
    }
}
