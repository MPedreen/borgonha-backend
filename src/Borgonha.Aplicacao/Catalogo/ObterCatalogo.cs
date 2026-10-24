namespace Borgonha.Aplicacao.Catalogo;

/// <summary>
/// Cardápio completo, agrupado por categoria e na ordem de exibição do site.
/// </summary>
public sealed class ObterCatalogo(ICatalogoRepositorio repositorio)
{
    public async Task<CatalogoDto> ExecutarAsync(CancellationToken cancellationToken)
    {
        var categorias = await repositorio.ListarCategoriasComProdutosAsync(cancellationToken).ConfigureAwait(false);

        return new CatalogoDto([.. categorias.OrderBy(categoria => categoria.Ordem).Select(categoria => categoria.ParaDto())]);
    }
}
