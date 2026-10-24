using Borgonha.Aplicacao.Lojas;
using Borgonha.Dominio.Lojas;
using Borgonha.Infraestrutura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Borgonha.Infraestrutura.Repositorios;

internal sealed class LojaRepositorio(BorgonhaDbContext contexto) : ILojaRepositorio
{
    public Task<Loja?> ObterAsync(CancellationToken cancellationToken) =>
        contexto.Lojas.AsNoTracking().OrderBy(loja => loja.Id).FirstOrDefaultAsync(cancellationToken);
}
