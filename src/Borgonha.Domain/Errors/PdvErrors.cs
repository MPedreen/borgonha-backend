namespace Borgonha.Domain.Errors;

public static class PdvErrors
{
    public static class Venda
    {
        public static readonly Error SemItens =
            Error.Validacao("Venda.SemItens", "A venda deve conter ao menos um item.");

        public static readonly Error ValorPagoInsuficiente =
            Error.Validacao("Venda.ValorPagoInsuficiente", "O valor pago é inferior ao total da venda.");

        public static readonly Error CriadoPorObrigatorio =
            Error.Validacao("Venda.CriadoPorObrigatorio", "O responsável pela venda é obrigatório.");

        public static readonly Error NaoEncontrada =
            Error.NaoEncontrado("Venda.NaoEncontrada", "Venda não encontrada.");
    }

    public static class Item
    {
        public static readonly Error QuantidadeInvalida =
            Error.Validacao("Item.QuantidadeInvalida", "A quantidade de cada item deve ser maior que zero.");

        public static readonly Error PrecoInvalido =
            Error.Validacao("Item.PrecoInvalido", "O preço unitário do item deve ser maior que zero.");

        public static readonly Error ProdutoDuplicado =
            Error.Validacao("Item.ProdutoDuplicado", "Não é permitido adicionar o mesmo produto mais de uma vez na venda.");
    }

    public static class Estoque
    {
        public static Error InsuficienteParaVenda(IEnumerable<string> ingredientes) =>
            Error.NaoProcessavel(
                "Estoque.InsuficienteParaVenda",
                $"Estoque insuficiente para: {string.Join(", ", ingredientes)}.");
    }
}
