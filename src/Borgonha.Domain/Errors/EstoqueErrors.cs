namespace Borgonha.Domain.Errors;

public static class EstoqueErrors
{
    public static class Ingrediente
    {
        public static readonly Error NomeObrigatorio =
            Error.Validacao("Ingrediente.NomeObrigatorio", "O nome do ingrediente é obrigatório.");

        public static readonly Error NomeMuitoLongo =
            Error.Validacao("Ingrediente.NomeMuitoLongo", "O nome do ingrediente deve ter no máximo 100 caracteres.");

        public static readonly Error UnidadeObrigatoria =
            Error.Validacao("Ingrediente.UnidadeObrigatoria", "A unidade de medida do ingrediente é obrigatória.");

        public static readonly Error QuantidadeMinimaInvalida =
            Error.Validacao("Ingrediente.QuantidadeMinimaInvalida", "A quantidade mínima não pode ser negativa.");

        public static readonly Error CustoInvalido =
            Error.Validacao("Ingrediente.CustoInvalido", "O custo unitário não pode ser negativo.");

        public static readonly Error NaoEncontrado =
            Error.NaoEncontrado("Ingrediente.NaoEncontrado", "Ingrediente não encontrado.");

        public static readonly Error QuantidadeEntradaInvalida =
            Error.Validacao("Ingrediente.QuantidadeEntradaInvalida", "A quantidade de entrada deve ser maior que zero.");

        public static readonly Error QuantidadeSaidaInvalida =
            Error.Validacao("Ingrediente.QuantidadeSaidaInvalida", "A quantidade de saída deve ser maior que zero.");

        public static Error EstoqueInsuficiente(string nome, decimal disponivel, decimal necessario) =>
            Error.Validacao(
                "Ingrediente.EstoqueInsuficiente",
                $"Estoque insuficiente para '{nome}': disponível {disponivel:F3}, necessário {necessario:F3}.");
    }
}
