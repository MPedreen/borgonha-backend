using Borgonha.Dominio.Comum;

namespace Borgonha.Aplicacao.Administracao;

public enum TipoDeFoto
{
    /// <summary>Produto inteiro (no caso dos cookies, visto de cima).</summary>
    Inteiro,

    /// <summary>Produto partido ao meio, mostrando o recheio.</summary>
    Partido,
}

public static class TiposDeFoto
{
    public static TipoDeFoto Ler(string? texto) => texto switch
    {
        "inteiro" => TipoDeFoto.Inteiro,
        "partido" => TipoDeFoto.Partido,
        _ => throw new DominioException("Tipo de foto inválido. Use \"inteiro\" ou \"partido\"."),
    };
}
