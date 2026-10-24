using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Borgonha.Dominio.Comum;

/// <summary>
/// Identificador legível usado em URLs e âncoras do site (ex.: "red-velvet").
/// </summary>
public sealed partial record Slug
{
    public const int TamanhoMaximo = 60;

    private Slug(string valor) => Valor = valor;

    public string Valor { get; }

    public static Slug Criar(string valor) =>
        TentarCriar(valor, out var slug) ? slug : throw new DominioException($"Slug inválido: '{valor}'.");

    public static bool TentarCriar([NotNullWhen(true)] string? valor, [NotNullWhen(true)] out Slug? slug)
    {
        slug = valor is { Length: > 0 and <= TamanhoMaximo } && Formato().IsMatch(valor) ? new Slug(valor) : null;
        return slug is not null;
    }

    public override string ToString() => Valor;

    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    private static partial Regex Formato();
}
