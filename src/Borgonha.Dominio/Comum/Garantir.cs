namespace Borgonha.Dominio.Comum;

/// <summary>
/// Validações reutilizadas pelas entidades e pelos objetos de valor.
/// </summary>
internal static class Garantir
{
    public static string TextoObrigatorio(string? valor, string campo, int tamanhoMaximo)
    {
        var texto = valor?.Trim();

        if (string.IsNullOrEmpty(texto))
        {
            throw new DominioException($"{campo} é obrigatório.");
        }

        return texto.Length <= tamanhoMaximo
            ? texto
            : throw new DominioException($"{campo} deve ter no máximo {tamanhoMaximo} caracteres.");
    }

    public static string? TextoOpcional(string? valor, string campo, int tamanhoMaximo) =>
        string.IsNullOrWhiteSpace(valor) ? null : TextoObrigatorio(valor, campo, tamanhoMaximo);

    public static int NaoNegativo(int valor, string campo) =>
        valor >= 0 ? valor : throw new DominioException($"{campo} não pode ser negativo.");

    public static Uri LinkHttps(Uri? link, string campo) =>
        link is { IsAbsoluteUri: true } && link.Scheme == Uri.UriSchemeHttps
            ? link
            : throw new DominioException($"{campo} deve ser um endereço HTTPS absoluto.");
}
