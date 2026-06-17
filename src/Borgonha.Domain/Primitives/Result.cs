using Borgonha.Domain.Errors;

namespace Borgonha.Domain.Primitives;

public class Result
{
    protected Result(bool sucesso, Error erro)
    {
        if (sucesso && erro != Error.Nenhum)
            throw new InvalidOperationException("Resultado bem-sucedido não pode conter erro.");

        if (!sucesso && erro == Error.Nenhum)
            throw new InvalidOperationException("Resultado falho deve conter um erro.");

        Sucesso = sucesso;
        Erro = erro;
    }

    public bool Sucesso { get; }
    public bool Falhou => !Sucesso;
    public Error Erro { get; }

    public static Result Ok() => new(true, Error.Nenhum);
    public static Result<TValue> Ok<TValue>(TValue valor) => new(valor, true, Error.Nenhum);
    public static Result Falha(Error erro) => new(false, erro);
    public static Result<TValue> Falha<TValue>(Error erro) => new(default, false, erro);
}

public sealed class Result<TValue> : Result
{
    private readonly TValue? _valor;

    internal Result(TValue? valor, bool sucesso, Error erro)
        : base(sucesso, erro)
    {
        _valor = valor;
    }

    public TValue Valor => Sucesso
        ? _valor!
        : throw new InvalidOperationException("Não é possível acessar o valor de um resultado falho.");

    public static implicit operator Result<TValue>(TValue valor) => Ok(valor);
}
