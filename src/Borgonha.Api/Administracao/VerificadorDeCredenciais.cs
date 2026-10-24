using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace Borgonha.Api.Administracao;

/// <summary>
/// Confere usuário e senha da área do administrador.
/// </summary>
internal sealed class VerificadorDeCredenciais(IOptionsMonitor<AdministradorOpcoes> opcoes)
{
    public bool Verificar(string? usuario, string? senha)
    {
        var configuracao = opcoes.CurrentValue;
        if (!configuracao.Configurado)
        {
            return false;
        }

        // Usuário e senha são sempre conferidos juntos, para o tempo de resposta não revelar qual dos dois errou.
        var usuarioConfere = UsuariosIguais(usuario ?? string.Empty, configuracao.Usuario!);
        var senhaConfere = HashDeSenha.Verificar(senha ?? string.Empty, configuracao.SenhaHash!);

        return usuarioConfere & senhaConfere;
    }

    /// <summary>
    /// Impressão digital do hash da senha atual. Vai na sessão: se a senha mudar, as sessões antigas caem.
    /// </summary>
    public string VersaoDaSenha() =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(opcoes.CurrentValue.SenhaHash ?? string.Empty)))[..16];

    /// <summary>
    /// Sem diferenciar maiúsculas (o celular costuma pôr a primeira letra em maiúscula) e em tempo constante.
    /// </summary>
    private static bool UsuariosIguais(string digitado, string configurado) =>
        CryptographicOperations.FixedTimeEquals(
            SHA256.HashData(Encoding.UTF8.GetBytes(digitado.Trim().ToUpperInvariant())),
            SHA256.HashData(Encoding.UTF8.GetBytes(configurado.Trim().ToUpperInvariant())));
}
