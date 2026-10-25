namespace Borgonha.Infraestrutura.Mapas;

/// <summary>
/// Serviço de mapas (Geoapify), seção "Geoapify" da configuração. A chave nunca fica no código nem no
/// appsettings: em desenvolvimento vai para os user-secrets; em produção, para a variável de ambiente
/// Geoapify__Chave. Sem chave, a consulta de endereço responde "indisponível" e o site manda o cliente
/// confirmar pelo WhatsApp. Veja o README.
/// </summary>
public sealed class OpcoesDaGeoapify
{
    public const string Secao = "Geoapify";

    public string? Chave { get; init; }

    public Uri UrlBase { get; init; } = new("https://api.geoapify.com/");

    /// <summary>
    /// Créditos que a API pode gastar por dia (UTC). O plano gratuito dá 3.000; o padrão deixa 10% de folga,
    /// porque o serviço não bloqueia ao passar do limite: ele pede upgrade e pode suspender a conta.
    /// </summary>
    public int CreditosPorDia { get; init; } = 2_700;

    /// <summary>Quanto esperar a resposta do serviço antes de desistir.</summary>
    public TimeSpan TempoLimite { get; init; } = TimeSpan.FromSeconds(6);

    public bool Configurada => !string.IsNullOrWhiteSpace(Chave);

    public bool EhValida() =>
        CreditosPorDia > 0
        && TempoLimite > TimeSpan.Zero
        && UrlBase.IsAbsoluteUri
        && UrlBase.Scheme == Uri.UriSchemeHttps;
}
