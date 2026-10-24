namespace Borgonha.Api.Configuracao;

/// <summary>
/// Domínios do site autorizados a chamar a API pelo navegador (seção "Cors" do appsettings).
/// </summary>
internal sealed class CorsOpcoes
{
    public const string Secao = "Cors";

    public IReadOnlyList<string> OrigensPermitidas { get; init; } = [];

    public bool OrigensSaoValidas() =>
        OrigensPermitidas.All(origem =>
            Uri.TryCreate(origem, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp)
            && uri.AbsolutePath == "/");
}
