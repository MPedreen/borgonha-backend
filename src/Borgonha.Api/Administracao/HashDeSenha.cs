using System.Globalization;
using System.Security.Cryptography;

namespace Borgonha.Api.Administracao;

/// <summary>
/// Hash de senha com PBKDF2-HMAC-SHA256, sal aleatório e 600 mil iterações (recomendação da OWASP).
/// Formato: <c>pbkdf2-sha256$iterações$sal$hash</c>, com sal e hash em Base64.
/// A senha nunca é guardada; só dá para conferir se uma senha digitada gera o mesmo hash.
/// </summary>
internal static class HashDeSenha
{
    public const int Iteracoes = 600_000;

    private const string Algoritmo = "pbkdf2-sha256";
    private const int IteracoesMinimas = 100_000;
    private const int TamanhoDoSal = 16;
    private const int TamanhoDoHash = 32;

    public static string Gerar(string senha)
    {
        ArgumentException.ThrowIfNullOrEmpty(senha);

        var sal = RandomNumberGenerator.GetBytes(TamanhoDoSal);
        var hash = Rfc2898DeriveBytes.Pbkdf2(senha, sal, Iteracoes, HashAlgorithmName.SHA256, TamanhoDoHash);

        return string.Join('$', Algoritmo, Iteracoes.ToString(CultureInfo.InvariantCulture), Convert.ToBase64String(sal), Convert.ToBase64String(hash));
    }

    public static bool EhValido(string? hashArmazenado) => TentarLer(hashArmazenado, out _, out _, out _);

    /// <summary>
    /// Confere a senha em tempo constante (não dá para descobrir nada pelo tempo de resposta).
    /// </summary>
    public static bool Verificar(string senha, string hashArmazenado)
    {
        ArgumentNullException.ThrowIfNull(senha);

        if (!TentarLer(hashArmazenado, out var iteracoes, out var sal, out var esperado))
        {
            return false;
        }

        var calculado = Rfc2898DeriveBytes.Pbkdf2(senha, sal, iteracoes, HashAlgorithmName.SHA256, esperado.Length);

        return CryptographicOperations.FixedTimeEquals(calculado, esperado);
    }

    private static bool TentarLer(string? valor, out int iteracoes, out byte[] sal, out byte[] hash)
    {
        iteracoes = 0;
        sal = [];
        hash = [];

        var partes = valor?.Split('$');
        if (partes is not [Algoritmo, var textoIteracoes, var textoSal, var textoHash]
            || !int.TryParse(textoIteracoes, NumberStyles.None, CultureInfo.InvariantCulture, out iteracoes)
            || iteracoes < IteracoesMinimas)
        {
            return false;
        }

        try
        {
            sal = Convert.FromBase64String(textoSal);
            hash = Convert.FromBase64String(textoHash);
        }
        catch (FormatException)
        {
            return false;
        }

        return sal.Length >= TamanhoDoSal && hash.Length >= TamanhoDoHash;
    }
}
