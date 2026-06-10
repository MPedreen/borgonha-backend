namespace Borgonha.Shared.Errors;

public sealed record Error(string Codigo, string Descricao, ErrorType Tipo)
{
    public static readonly Error Nenhum = new(string.Empty, string.Empty, ErrorType.Unexpected);

    public static Error Validacao(string codigo, string descricao)
        => new(codigo, descricao, ErrorType.Validation);

    public static Error NaoEncontrado(string codigo, string descricao)
        => new(codigo, descricao, ErrorType.NotFound);

    public static Error Conflito(string codigo, string descricao)
        => new(codigo, descricao, ErrorType.Conflict);

    public static Error NaoAutorizado(string codigo, string descricao)
        => new(codigo, descricao, ErrorType.Unauthorized);

    public static Error Inesperado(string codigo, string descricao)
        => new(codigo, descricao, ErrorType.Unexpected);
}
