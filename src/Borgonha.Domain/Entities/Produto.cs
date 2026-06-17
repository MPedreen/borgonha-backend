using Borgonha.Domain.Errors;
using Borgonha.Domain.Primitives;

namespace Borgonha.Domain.Entities;

public sealed class Produto : Entity
{
    private readonly List<ReceitaItem> _receita = [];

    private Produto() { }

    private Produto(Guid id, string nome, decimal precoVenda, DateTime criadoEm)
        : base(id)
    {
        Nome = nome;
        PrecoVenda = precoVenda;
        Ativo = true;
        CriadoEm = criadoEm;
    }

    public string Nome { get; private set; } = string.Empty;
    public decimal PrecoVenda { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public IReadOnlyCollection<ReceitaItem> Receita => _receita.AsReadOnly();

    public bool PossuiReceita => _receita.Count > 0;

    public static Result<Produto> Criar(string nome, decimal precoVenda)
    {
        if (string.IsNullOrWhiteSpace(nome))
            return Result.Falha<Produto>(ProdutosErrors.Produto.NomeObrigatorio);

        if (nome.Length > 100)
            return Result.Falha<Produto>(ProdutosErrors.Produto.NomeMuitoLongo);

        if (precoVenda <= 0)
            return Result.Falha<Produto>(ProdutosErrors.Produto.PrecoInvalido);

        return Result.Ok(new Produto(Guid.NewGuid(), nome.Trim(), precoVenda, DateTime.UtcNow));
    }

    public Result Atualizar(string nome, decimal precoVenda)
    {
        if (string.IsNullOrWhiteSpace(nome))
            return Result.Falha(ProdutosErrors.Produto.NomeObrigatorio);

        if (nome.Length > 100)
            return Result.Falha(ProdutosErrors.Produto.NomeMuitoLongo);

        if (precoVenda <= 0)
            return Result.Falha(ProdutosErrors.Produto.PrecoInvalido);

        Nome = nome.Trim();
        PrecoVenda = precoVenda;
        return Result.Ok();
    }

    public Result Desativar()
    {
        if (!Ativo)
            return Result.Falha(ProdutosErrors.Produto.JaInativo);

        Ativo = false;
        return Result.Ok();
    }

    public Result AdicionarItemReceita(Guid ingredienteId, decimal quantidade)
    {
        if (quantidade <= 0)
            return Result.Falha(ProdutosErrors.Receita.QuantidadeInvalida);

        if (_receita.Any(r => r.IngredienteId == ingredienteId))
            return Result.Falha(ProdutosErrors.Receita.IngredienteJaAdicionado);

        _receita.Add(ReceitaItem.Criar(Id, ingredienteId, quantidade));
        return Result.Ok();
    }

    public Result RemoverItemReceita(Guid ingredienteId)
    {
        var item = _receita.FirstOrDefault(r => r.IngredienteId == ingredienteId);
        if (item is null)
            return Result.Falha(ProdutosErrors.Receita.ItemNaoEncontrado);

        _receita.Remove(item);
        return Result.Ok();
    }
}
