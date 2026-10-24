using Borgonha.Aplicacao.Duvidas;
using Borgonha.Dominio.Duvidas;
using Borgonha.Infraestrutura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Borgonha.Infraestrutura.Repositorios;

internal sealed class DuvidaRepositorio(BorgonhaDbContext contexto) : IDuvidaRepositorio
{
    public async Task<IReadOnlyList<Duvida>> ListarAsync(CancellationToken cancellationToken) =>
        await contexto.Duvidas
            .AsNoTracking()
            .OrderBy(duvida => duvida.Ordem)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
}
