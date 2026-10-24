using Borgonha.Aplicacao.Catalogo;

namespace Borgonha.Aplicacao.Administracao;

/// <summary>
/// Todos os produtos, inclusive os ocultos, na ordem de exibição do site.
/// </summary>
public sealed class ListarCardapioAdmin(ICatalogoRepositorio repositorio)
{
    public async Task<CardapioAdminDto> ExecutarAsync(CancellationToken cancellationToken)
    {
        var categorias = await repositorio.ListarCategoriasComProdutosAsync(cancellationToken).ConfigureAwait(false);

        return new CardapioAdminDto([.. categorias.OrderBy(categoria => categoria.Ordem).Select(categoria => categoria.ParaDtoAdmin())]);
    }
}
