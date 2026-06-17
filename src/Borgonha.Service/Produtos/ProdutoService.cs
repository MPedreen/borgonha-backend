using Borgonha.Domain.Entities;
using Borgonha.Domain.Errors;
using Borgonha.Domain.Primitives;
using Borgonha.Domain.Repositories;
using Borgonha.Service.Produtos.Dtos;

namespace Borgonha.Service.Produtos;

internal sealed class ProdutoService(
    IProdutoRepository produtoRepository,
    IIngredienteRepository ingredienteRepository,
    IUnitOfWork unitOfWork) : IProdutoService
{
    public async Task<Result<ProdutoDto>> CriarAsync(CriarProdutoRequest request, CancellationToken cancellationToken = default)
    {
        var resultadoProduto = Produto.Criar(request.Nome, request.PrecoVenda);
        if (resultadoProduto.Falhou)
            return Result.Falha<ProdutoDto>(resultadoProduto.Erro);

        var produto = resultadoProduto.Valor;

        if (request.Receita.Count > 0)
        {
            var ingredienteIds = request.Receita.Select(r => r.IngredienteId).Distinct().ToList();
            var ingredientes = await ingredienteRepository.ObterPorIdsAsync(ingredienteIds, cancellationToken);

            if (ingredientes.Count != ingredienteIds.Count)
                return Result.Falha<ProdutoDto>(EstoqueErrors.Ingrediente.NaoEncontrado);

            foreach (var item in request.Receita)
            {
                var resultadoItem = produto.AdicionarItemReceita(item.IngredienteId, item.Quantidade);
                if (resultadoItem.Falhou)
                    return Result.Falha<ProdutoDto>(resultadoItem.Erro);
            }
        }

        if (!produto.PossuiReceita)
            return Result.Falha<ProdutoDto>(ProdutosErrors.Produto.SemReceita);

        await produtoRepository.AdicionarAsync(produto, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok(ParaDto(produto));
    }

    public async Task<Result<ProdutoDto>> AtualizarAsync(Guid id, AtualizarProdutoRequest request, CancellationToken cancellationToken = default)
    {
        var produto = await produtoRepository.ObterPorIdAsync(id, cancellationToken);
        if (produto is null)
            return Result.Falha<ProdutoDto>(ProdutosErrors.Produto.NaoEncontrado);

        var resultado = produto.Atualizar(request.Nome, request.PrecoVenda);
        if (resultado.Falhou)
            return Result.Falha<ProdutoDto>(resultado.Erro);

        produtoRepository.Atualizar(produto);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok(ParaDto(produto));
    }

    public async Task<Result<IReadOnlyList<ProdutoDto>>> ObterAtivosAsync(CancellationToken cancellationToken = default)
    {
        var produtos = await produtoRepository.ObterAtivosAsync(cancellationToken);
        return Result.Ok<IReadOnlyList<ProdutoDto>>(produtos.Select(ParaDto).ToList());
    }

    private static ProdutoDto ParaDto(Produto produto) => new(
        produto.Id,
        produto.Nome,
        produto.PrecoVenda,
        produto.Ativo,
        produto.CriadoEm,
        produto.Receita.Select(r => new ItemReceitaDto(r.IngredienteId, r.Quantidade)).ToList());
}
