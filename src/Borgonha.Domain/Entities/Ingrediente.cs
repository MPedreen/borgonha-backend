using Borgonha.Domain.Errors;
using Borgonha.Domain.Primitives;

namespace Borgonha.Domain.Entities;

public sealed class Ingrediente : Entity
{
    private readonly List<MovimentacaoEstoque> _movimentacoes = [];

    private Ingrediente() { }

    private Ingrediente(Guid id, string nome, string unidade, decimal quantidadeMinima, decimal custoUnitario, DateTime criadoEm)
        : base(id)
    {
        Nome = nome;
        Unidade = unidade;
        QuantidadeAtual = 0;
        QuantidadeMinima = quantidadeMinima;
        CustoUnitario = custoUnitario;
        CriadoEm = criadoEm;
    }

    public string Nome { get; private set; } = string.Empty;
    public string Unidade { get; private set; } = string.Empty;
    public decimal QuantidadeAtual { get; private set; }
    public decimal QuantidadeMinima { get; private set; }
    public decimal CustoUnitario { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public IReadOnlyCollection<MovimentacaoEstoque> Movimentacoes => _movimentacoes.AsReadOnly();

    public bool EstaEmAlerta => QuantidadeAtual <= QuantidadeMinima;
    public bool TemEstoqueSuficiente(decimal quantidade) => QuantidadeAtual >= quantidade;

    public static Result<Ingrediente> Criar(string nome, string unidade, decimal quantidadeMinima, decimal custoUnitario)
    {
        if (string.IsNullOrWhiteSpace(nome))
            return Result.Falha<Ingrediente>(EstoqueErrors.Ingrediente.NomeObrigatorio);

        if (nome.Length > 100)
            return Result.Falha<Ingrediente>(EstoqueErrors.Ingrediente.NomeMuitoLongo);

        if (string.IsNullOrWhiteSpace(unidade))
            return Result.Falha<Ingrediente>(EstoqueErrors.Ingrediente.UnidadeObrigatoria);

        if (quantidadeMinima < 0)
            return Result.Falha<Ingrediente>(EstoqueErrors.Ingrediente.QuantidadeMinimaInvalida);

        if (custoUnitario < 0)
            return Result.Falha<Ingrediente>(EstoqueErrors.Ingrediente.CustoInvalido);

        return Result.Ok(new Ingrediente(Guid.NewGuid(), nome.Trim(), unidade.Trim(), quantidadeMinima, custoUnitario, DateTime.UtcNow));
    }

    public Result Atualizar(string nome, string unidade, decimal quantidadeMinima, decimal custoUnitario)
    {
        if (string.IsNullOrWhiteSpace(nome))
            return Result.Falha(EstoqueErrors.Ingrediente.NomeObrigatorio);

        if (string.IsNullOrWhiteSpace(unidade))
            return Result.Falha(EstoqueErrors.Ingrediente.UnidadeObrigatoria);

        if (quantidadeMinima < 0)
            return Result.Falha(EstoqueErrors.Ingrediente.QuantidadeMinimaInvalida);

        if (custoUnitario < 0)
            return Result.Falha(EstoqueErrors.Ingrediente.CustoInvalido);

        Nome = nome.Trim();
        Unidade = unidade.Trim();
        QuantidadeMinima = quantidadeMinima;
        CustoUnitario = custoUnitario;
        return Result.Ok();
    }

    public Result RegistrarEntrada(decimal quantidade, string? observacao)
    {
        if (quantidade <= 0)
            return Result.Falha(EstoqueErrors.Ingrediente.QuantidadeEntradaInvalida);

        QuantidadeAtual += quantidade;
        _movimentacoes.Add(MovimentacaoEstoque.CriarEntrada(Id, quantidade, observacao));
        return Result.Ok();
    }

    public Result BaixarEstoque(decimal quantidade, Guid vendaId)
    {
        if (quantidade <= 0)
            return Result.Falha(EstoqueErrors.Ingrediente.QuantidadeSaidaInvalida);

        if (!TemEstoqueSuficiente(quantidade))
            return Result.Falha(EstoqueErrors.Ingrediente.EstoqueInsuficiente(Nome, QuantidadeAtual, quantidade));

        QuantidadeAtual -= quantidade;
        _movimentacoes.Add(MovimentacaoEstoque.CriarSaida(Id, quantidade, vendaId));
        return Result.Ok();
    }
}
