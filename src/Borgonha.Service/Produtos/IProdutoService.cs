using Borgonha.Domain.Primitives;
using Borgonha.Service.Produtos.Dtos;

namespace Borgonha.Service.Produtos;

public interface IProdutoService
{
    Task<Result<ProdutoDto>> CriarAsync(CriarProdutoRequest request, CancellationToken cancellationToken = default);
    Task<Result<ProdutoDto>> AtualizarAsync(Guid id, AtualizarProdutoRequest request, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ProdutoDto>>> ObterAtivosAsync(CancellationToken cancellationToken = default);
}
