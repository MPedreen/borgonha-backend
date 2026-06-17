using Borgonha.Domain.Entities;
using Borgonha.Domain.Errors;
using Borgonha.Domain.Primitives;
using Borgonha.Domain.Repositories;
using Borgonha.Service.Pdv.Dtos;

namespace Borgonha.Service.Pdv;

internal sealed class VendaService(
    IProdutoRepository produtoRepository,
    IIngredienteRepository ingredienteRepository,
    IVendaRepository vendaRepository,
    IUnitOfWork unitOfWork) : IVendaService
{
    public async Task<Result<VendaDto>> RegistrarAsync(string criadoPor, RegistrarVendaRequest request, CancellationToken cancellationToken = default)
    {
        var produtosPorId = new Dictionary<Guid, Produto>();

        foreach (var produtoId in request.Itens.Select(i => i.ProdutoId).Distinct())
        {
            var produto = await produtoRepository.ObterPorIdAsync(produtoId, cancellationToken);
            if (produto is null)
                return Result.Falha<VendaDto>(ProdutosErrors.Produto.NaoEncontrado);

            produtosPorId[produtoId] = produto;
        }

        var itensVenda = request.Itens
            .Select(i => (i.ProdutoId, i.Quantidade, produtosPorId[i.ProdutoId].PrecoVenda))
            .ToList();

        var resultadoVenda = Venda.Criar(criadoPor, request.ValorPago, itensVenda);
        if (resultadoVenda.Falhou)
            return Result.Falha<VendaDto>(resultadoVenda.Erro);

        var venda = resultadoVenda.Valor;

        var quantidadeNecessariaPorIngrediente = new Dictionary<Guid, decimal>();

        foreach (var item in request.Itens)
        {
            foreach (var receitaItem in produtosPorId[item.ProdutoId].Receita)
            {
                var necessario = receitaItem.Quantidade * item.Quantidade;
                quantidadeNecessariaPorIngrediente[receitaItem.IngredienteId] =
                    quantidadeNecessariaPorIngrediente.GetValueOrDefault(receitaItem.IngredienteId) + necessario;
            }
        }

        var ingredientes = await ingredienteRepository.ObterPorIdsAsync(quantidadeNecessariaPorIngrediente.Keys, cancellationToken);
        var ingredientesPorId = ingredientes.ToDictionary(i => i.Id);

        var ingredientesBloqueantes = quantidadeNecessariaPorIngrediente
            .Where(par => !ingredientesPorId[par.Key].TemEstoqueSuficiente(par.Value))
            .Select(par => ingredientesPorId[par.Key].Nome)
            .ToList();

        if (ingredientesBloqueantes.Count > 0)
            return Result.Falha<VendaDto>(PdvErrors.Estoque.InsuficienteParaVenda(ingredientesBloqueantes));

        foreach (var (ingredienteId, quantidadeNecessaria) in quantidadeNecessariaPorIngrediente)
        {
            var ingrediente = ingredientesPorId[ingredienteId];
            var resultadoBaixa = ingrediente.BaixarEstoque(quantidadeNecessaria, venda.Id);
            if (resultadoBaixa.Falhou)
                return Result.Falha<VendaDto>(resultadoBaixa.Erro);

            ingredienteRepository.Atualizar(ingrediente);
        }

        await vendaRepository.AdicionarAsync(venda, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok(ParaDto(venda));
    }

    public async Task<Result<VendaDto>> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var venda = await vendaRepository.ObterPorIdAsync(id, cancellationToken);
        if (venda is null)
            return Result.Falha<VendaDto>(PdvErrors.Venda.NaoEncontrada);

        return Result.Ok(ParaDto(venda));
    }

    private static VendaDto ParaDto(Venda venda) => new(
        venda.Id,
        venda.CriadoPor,
        venda.Total,
        venda.ValorPago,
        venda.Troco,
        venda.DataHora,
        venda.Itens.Select(i => new ItemVendaDto(i.ProdutoId, i.Quantidade, i.PrecoUnitario, i.Subtotal)).ToList());
}
