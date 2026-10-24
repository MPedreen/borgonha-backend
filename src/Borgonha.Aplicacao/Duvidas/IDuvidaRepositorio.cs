using Borgonha.Dominio.Duvidas;

namespace Borgonha.Aplicacao.Duvidas;

public interface IDuvidaRepositorio
{
    Task<IReadOnlyList<Duvida>> ListarAsync(CancellationToken cancellationToken);
}
