using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
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

    /// <summary>
    /// Monta um slug a partir de um texto livre, sem acentos nem símbolos
    /// (ex.: "Brigadeiro de Maracujá" → "brigadeiro-de-maracuja").
    /// </summary>
    public static Slug GerarAPartirDe(string texto)
    {
        ArgumentNullException.ThrowIfNull(texto);

        var semAcentos = new string([.. texto
            .Normalize(NormalizationForm.FormD)
            .Where(letra => CharUnicodeInfo.GetUnicodeCategory(letra) != UnicodeCategory.NonSpacingMark)]);
        var valor = ForaDoFormato().Replace(semAcentos.ToLowerInvariant(), "-").Trim('-');

        if (valor.Length > TamanhoMaximo)
        {
            valor = valor[..TamanhoMaximo].TrimEnd('-');
        }

        return Criar(valor.Length > 0 ? valor : "produto");
    }

    /// <summary>
    /// O mesmo slug com um número no fim (ex.: "brookie-2"), para desempatar slugs repetidos.
    /// </summary>
    public Slug ComSufixo(int numero)
    {
        var sufixo = $"-{numero}";
        var inicio = Valor.Length + sufixo.Length > TamanhoMaximo
            ? Valor[..(TamanhoMaximo - sufixo.Length)].TrimEnd('-')
            : Valor;

        return Criar(inicio + sufixo);
    }

    public override string ToString() => Valor;

    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    private static partial Regex Formato();

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex ForaDoFormato();
}
