namespace Borgonha.Api.Administracao;

/// <summary>
/// Acesso à área do administrador (seção "Administrador" da configuração).
/// O usuário e o hash da senha nunca ficam no código nem no appsettings: em desenvolvimento,
/// vão para os user-secrets; em produção, para variáveis de ambiente (Administrador__Usuario e
/// Administrador__SenhaHash). Veja o README.
/// </summary>
internal sealed class AdministradorOpcoes
{
    public const string Secao = "Administrador";

    public string? Usuario { get; init; }

    /// <summary>
    /// Hash PBKDF2 da senha, gerado por <c>dotnet run --project src/Borgonha.Api -- gerar-hash-senha</c>.
    /// </summary>
    public string? SenhaHash { get; init; }

    /// <summary>Tentativas de login aceitas de um mesmo endereço IP a cada 15 minutos.</summary>
    public int TentativasPorEndereco { get; init; } = 5;

    /// <summary>Tentativas de login aceitas no total, somando todos os endereços, a cada hora.</summary>
    public int TentativasPorHora { get; init; } = 20;

    public TimeSpan DuracaoDaSessao { get; init; } = TimeSpan.FromHours(8);

    public bool Configurado => !string.IsNullOrWhiteSpace(Usuario) && !string.IsNullOrWhiteSpace(SenhaHash);

    public bool EhValido() =>
        (string.IsNullOrWhiteSpace(SenhaHash) || HashDeSenha.EhValido(SenhaHash))
        && TentativasPorEndereco > 0
        && TentativasPorHora > 0
        && DuracaoDaSessao > TimeSpan.Zero;
}
