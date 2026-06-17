namespace Borgonha.Domain.Errors;

public static class ProdutosErrors
{
    public static class Produto
    {
        public static readonly Error NomeObrigatorio =
            Error.Validacao("Produto.NomeObrigatorio", "O nome do produto é obrigatório.");

        public static readonly Error NomeMuitoLongo =
            Error.Validacao("Produto.NomeMuitoLongo", "O nome do produto deve ter no máximo 100 caracteres.");

        public static readonly Error PrecoInvalido =
            Error.Validacao("Produto.PrecoInvalido", "O preço de venda deve ser maior que zero.");

        public static readonly Error NaoEncontrado =
            Error.NaoEncontrado("Produto.NaoEncontrado", "Produto não encontrado.");

        public static readonly Error JaInativo =
            Error.Conflito("Produto.JaInativo", "O produto já está inativo.");

        public static readonly Error SemReceita =
            Error.Validacao("Produto.SemReceita", "O produto precisa ter ao menos um ingrediente na receita para ser vendido.");
    }

    public static class Receita
    {
        public static readonly Error QuantidadeInvalida =
            Error.Validacao("Receita.QuantidadeInvalida", "A quantidade do ingrediente na receita deve ser maior que zero.");

        public static readonly Error IngredienteJaAdicionado =
            Error.Conflito("Receita.IngredienteJaAdicionado", "Este ingrediente já faz parte da receita do produto.");

        public static readonly Error ItemNaoEncontrado =
            Error.NaoEncontrado("Receita.ItemNaoEncontrado", "Item da receita não encontrado.");
    }
}
