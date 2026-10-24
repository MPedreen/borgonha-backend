using Borgonha.Dominio.Lojas;

namespace Borgonha.Aplicacao.Lojas;

public interface ILojaRepositorio
{
    Task<Loja?> ObterAsync(CancellationToken cancellationToken);
}
