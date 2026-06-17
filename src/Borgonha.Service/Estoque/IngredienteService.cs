using Borgonha.Domain.Entities;
using Borgonha.Domain.Errors;
using Borgonha.Domain.Primitives;
using Borgonha.Domain.Repositories;
using Borgonha.Service.Estoque.Dtos;

namespace Borgonha.Service.Estoque;

internal sealed class IngredienteService(
    IIngredienteRepository ingredienteRepository,
    IUnitOfWork unitOfWork) : IIngredienteService
{
    public async Task<Result<IngredienteDto>> CriarAsync(CriarIngredienteRequest request, CancellationToken cancellationToken = default)
    {
        var resultado = Ingrediente.Criar(request.Nome, request.Unidade, request.QuantidadeMinima, request.CustoUnitario);
        if (resultado.Falhou)
            return Result.Falha<IngredienteDto>(resultado.Erro);

        var ingrediente = resultado.Valor;

        if (request.QuantidadeAtual > 0)
        {
            var resultadoEntrada = ingrediente.RegistrarEntrada(request.QuantidadeAtual, "Estoque inicial");
            if (resultadoEntrada.Falhou)
                return Result.Falha<IngredienteDto>(resultadoEntrada.Erro);
        }

        await ingredienteRepository.AdicionarAsync(ingrediente, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok(ParaDto(ingrediente));
    }

    public async Task<Result<IngredienteDto>> AtualizarAsync(Guid id, AtualizarIngredienteRequest request, CancellationToken cancellationToken = default)
    {
        var ingrediente = await ingredienteRepository.ObterPorIdAsync(id, cancellationToken);
        if (ingrediente is null)
            return Result.Falha<IngredienteDto>(EstoqueErrors.Ingrediente.NaoEncontrado);

        var resultado = ingrediente.Atualizar(request.Nome, request.Unidade, request.QuantidadeMinima, request.CustoUnitario);
        if (resultado.Falhou)
            return Result.Falha<IngredienteDto>(resultado.Erro);

        ingredienteRepository.Atualizar(ingrediente);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok(ParaDto(ingrediente));
    }

    public async Task<Result<IngredienteDto>> RegistrarEntradaAsync(Guid id, RegistrarEntradaRequest request, CancellationToken cancellationToken = default)
    {
        var ingrediente = await ingredienteRepository.ObterPorIdAsync(id, cancellationToken);
        if (ingrediente is null)
            return Result.Falha<IngredienteDto>(EstoqueErrors.Ingrediente.NaoEncontrado);

        var resultado = ingrediente.RegistrarEntrada(request.Quantidade, request.Observacao);
        if (resultado.Falhou)
            return Result.Falha<IngredienteDto>(resultado.Erro);

        ingredienteRepository.Atualizar(ingrediente);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok(ParaDto(ingrediente));
    }

    public async Task<Result<IReadOnlyList<IngredienteDto>>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        var ingredientes = await ingredienteRepository.ObterTodosAsync(cancellationToken);
        return Result.Ok<IReadOnlyList<IngredienteDto>>(ingredientes.Select(ParaDto).ToList());
    }

    public async Task<Result<IReadOnlyList<IngredienteDto>>> ObterEmAlertaAsync(CancellationToken cancellationToken = default)
    {
        var ingredientes = await ingredienteRepository.ObterEmAlertaAsync(cancellationToken);
        return Result.Ok<IReadOnlyList<IngredienteDto>>(ingredientes.Select(ParaDto).ToList());
    }

    private static IngredienteDto ParaDto(Ingrediente ingrediente) => new(
        ingrediente.Id,
        ingrediente.Nome,
        ingrediente.Unidade,
        ingrediente.QuantidadeAtual,
        ingrediente.QuantidadeMinima,
        ingrediente.CustoUnitario,
        ingrediente.CriadoEm,
        ingrediente.EstaEmAlerta);
}
