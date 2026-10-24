using System.Text.RegularExpressions;
using Borgonha.Dominio.Comum;

namespace Borgonha.Dominio.Lojas;

public sealed partial record Endereco
{
    public const int TamanhoMaximoLogradouro = 120;
    public const int TamanhoMaximoNumero = 10;
    public const int TamanhoMaximoBairro = 60;
    public const int TamanhoMaximoCidade = 60;

    public Endereco(string logradouro, string numero, string bairro, string cidade, string uf, string? cep = null)
    {
        Logradouro = Garantir.TextoObrigatorio(logradouro, "Logradouro", TamanhoMaximoLogradouro);
        Numero = Garantir.TextoObrigatorio(numero, "Número", TamanhoMaximoNumero);
        Bairro = Garantir.TextoObrigatorio(bairro, "Bairro", TamanhoMaximoBairro);
        Cidade = Garantir.TextoObrigatorio(cidade, "Cidade", TamanhoMaximoCidade);

        uf = Garantir.TextoObrigatorio(uf, "UF", 2);
        Uf = FormatoUf().IsMatch(uf) ? uf : throw new DominioException("A UF deve ter duas letras maiúsculas, por exemplo RJ.");

        cep = Garantir.TextoOpcional(cep, "CEP", 9);
        Cep = cep is null || FormatoCep().IsMatch(cep) ? cep : throw new DominioException("O CEP deve ter o formato 00000-000.");
    }

    public string Logradouro { get; }

    public string Numero { get; }

    public string Bairro { get; }

    public string Cidade { get; }

    public string Uf { get; }

    public string? Cep { get; }

    [GeneratedRegex("^[A-Z]{2}$")]
    private static partial Regex FormatoUf();

    [GeneratedRegex(@"^\d{5}-\d{3}$")]
    private static partial Regex FormatoCep();
}
