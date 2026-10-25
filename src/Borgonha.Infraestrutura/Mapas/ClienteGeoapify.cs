using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using Borgonha.Aplicacao.Entrega;
using Borgonha.Dominio.Lojas;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Borgonha.Infraestrutura.Mapas;

/// <summary>
/// Endereços (autocompletar) e trajetos pelo Geoapify. Cada chamada gasta 1 crédito do plano gratuito;
/// por isso há cache, contagem diária e nenhuma nova tentativa automática.
/// </summary>
internal sealed partial class ClienteGeoapify(
    HttpClient http,
    IOptions<OpcoesDaGeoapify> opcoes,
    ControleDeCreditos creditos,
    CacheDeMapas cache,
    ILogger<ClienteGeoapify> logger) : IGeocodificador, IRotas
{
    private const int MaximoDeSugestoes = 5;
    private static readonly TimeSpan DuracaoDosEnderecos = TimeSpan.FromHours(24);
    private static readonly TimeSpan DuracaoDosTrajetos = TimeSpan.FromDays(7);
    private static readonly TimeSpan PausaPorExcesso = TimeSpan.FromMinutes(1);

    public async Task<IReadOnlyList<EnderecoSugeridoDto>> BuscarAsync(string texto, Coordenadas perto, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(perto);

        var chave = $"endereco:{Normalizar(texto)}";
        if (cache.TentarObter<IReadOnlyList<EnderecoSugeridoDto>>(chave, out var guardados) && guardados is not null)
        {
            return guardados;
        }

        var url = string.Create(
            CultureInfo.InvariantCulture,
            $"v1/geocode/autocomplete?text={Uri.EscapeDataString(texto)}&filter=countrycode:br&bias=proximity:{perto.Longitude},{perto.Latitude}&lang=pt&limit={MaximoDeSugestoes}");

        using var resposta = await EnviarAsync(url, cancellationToken).ConfigureAwait(false);
        var sugestoes = await LerAsync(resposta, LerEnderecos, cancellationToken).ConfigureAwait(false);

        cache.Guardar(chave, sugestoes, DuracaoDosEnderecos);
        return sugestoes;
    }

    public async Task<int> DistanciaEmMetrosAsync(Coordenadas origem, Coordenadas destino, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(origem);
        ArgumentNullException.ThrowIfNull(destino);

        // Quatro casas decimais são cerca de 11 m: pontos tão próximos compartilham o trajeto.
        var chave = string.Create(
            CultureInfo.InvariantCulture,
            $"trajeto:{origem.Latitude:F4},{origem.Longitude:F4}>{destino.Latitude:F4},{destino.Longitude:F4}");
        if (cache.TentarObter<int>(chave, out var guardada))
        {
            return guardada;
        }

        var url = string.Create(
            CultureInfo.InvariantCulture,
            $"v1/routing?waypoints={origem.Latitude},{origem.Longitude}%7C{destino.Latitude},{destino.Longitude}&mode=drive");

        using var resposta = await EnviarAsync(url, cancellationToken, recusaEhRotaInexistente: true).ConfigureAwait(false);
        var metros = await LerAsync(resposta, LerDistancia, cancellationToken).ConfigureAwait(false);

        cache.Guardar(chave, metros, DuracaoDosTrajetos);
        return metros;
    }

    private async Task<HttpResponseMessage> EnviarAsync(string url, CancellationToken cancellationToken, bool recusaEhRotaInexistente = false)
    {
        var configuracao = opcoes.Value;
        if (!configuracao.Configurada)
        {
            RegistrarSemChave(logger);
            throw new MapasIndisponiveisException("A chave do serviço de mapas não foi configurada.");
        }

        if (!creditos.TentarUsar())
        {
            RegistrarSemCreditos(logger, creditos.Gastos);
            throw new MapasIndisponiveisException("O limite diário do serviço de mapas foi atingido.");
        }

        using var requisicao = new HttpRequestMessage(HttpMethod.Get, url);
        requisicao.Headers.Add("X-API-Key", configuracao.Chave);

        HttpResponseMessage resposta;
        try
        {
            resposta = await http.SendAsync(requisicao, cancellationToken).ConfigureAwait(false);
        }
        catch (HttpRequestException erro)
        {
            RegistrarFalha(logger, erro.GetType().Name);
            throw new MapasIndisponiveisException("O serviço de mapas não respondeu.", erro);
        }
        catch (TaskCanceledException erro) when (!cancellationToken.IsCancellationRequested)
        {
            RegistrarFalha(logger, "tempo esgotado");
            throw new MapasIndisponiveisException("O serviço de mapas demorou demais para responder.", erro);
        }

        if (resposta.IsSuccessStatusCode)
        {
            return resposta;
        }

        var status = resposta.StatusCode;
        resposta.Dispose();

        if (recusaEhRotaInexistente && status == HttpStatusCode.BadRequest)
        {
            throw new RotaNaoEncontradaException();
        }

        if (status == HttpStatusCode.TooManyRequests)
        {
            creditos.Pausar(PausaPorExcesso);
        }

        RegistrarRecusa(logger, (int)status);
        throw new MapasIndisponiveisException($"O serviço de mapas recusou a chamada ({(int)status}).");
    }

    private async Task<T> LerAsync<T>(HttpResponseMessage resposta, Func<JsonElement, T> ler, CancellationToken cancellationToken)
    {
        try
        {
            var conteudo = await resposta.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            await using (conteudo.ConfigureAwait(false))
            {
                using var json = await JsonDocument.ParseAsync(conteudo, cancellationToken: cancellationToken).ConfigureAwait(false);
                return ler(json.RootElement);
            }
        }
        catch (Exception erro) when (erro is JsonException or InvalidOperationException or KeyNotFoundException or FormatException)
        {
            RegistrarFalha(logger, "resposta inesperada");
            throw new MapasIndisponiveisException("O serviço de mapas respondeu algo que não entendemos.", erro);
        }
    }

    private static IReadOnlyList<EnderecoSugeridoDto> LerEnderecos(JsonElement raiz)
    {
        var sugestoes = new List<EnderecoSugeridoDto>();
        var vistos = new HashSet<string>(StringComparer.Ordinal);

        foreach (var item in raiz.GetProperty("features").EnumerateArray())
        {
            var propriedades = item.GetProperty("properties");
            if (!propriedades.TryGetProperty("lat", out var latitude) || !propriedades.TryGetProperty("lon", out var longitude))
            {
                continue;
            }

            var titulo = Texto(propriedades, "address_line1") ?? Texto(propriedades, "formatted");
            if (titulo is null)
            {
                continue;
            }

            var detalhe = (Texto(propriedades, "address_line2") ?? string.Empty).Replace(", Brasil", string.Empty, StringComparison.Ordinal);
            if (vistos.Add($"{titulo}|{detalhe}"))
            {
                sugestoes.Add(new EnderecoSugeridoDto(titulo, detalhe, latitude.GetDouble(), longitude.GetDouble()));
            }
        }

        return sugestoes;
    }

    private static int LerDistancia(JsonElement raiz)
    {
        var metros = raiz.GetProperty("features")[0].GetProperty("properties").GetProperty("distance").GetDouble();
        return (int)Math.Round(metros);
    }

    private static string? Texto(JsonElement propriedades, string nome) =>
        propriedades.TryGetProperty(nome, out var valor) && valor.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(valor.GetString())
            ? valor.GetString()
            : null;

    /// <summary>Minúsculas, sem acentos e sem pontuação: "Rua  Afrânio, 40" e "rua afranio 40" são a mesma consulta.</summary>
    private static string Normalizar(string texto)
    {
        var resultado = new StringBuilder(texto.Length);
        var separador = false;

        foreach (var letra in texto.Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(letra) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsLetterOrDigit(letra))
            {
                if (separador && resultado.Length > 0)
                {
                    resultado.Append(' ');
                }

                resultado.Append(char.ToLowerInvariant(letra));
                separador = false;
            }
            else
            {
                separador = true;
            }
        }

        return resultado.ToString();
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "A chave do Geoapify não está configurada (Geoapify:Chave): a consulta de endereço fica indisponível.")]
    private static partial void RegistrarSemChave(ILogger logger);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Limite diário de créditos do Geoapify atingido ({Gastos} gastos): a consulta de endereço fica indisponível até amanhã (UTC).")]
    private static partial void RegistrarSemCreditos(ILogger logger, int gastos);

    [LoggerMessage(Level = LogLevel.Warning, Message = "O Geoapify recusou a chamada com o status {Status}.")]
    private static partial void RegistrarRecusa(ILogger logger, int status);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Falha ao chamar o Geoapify: {Motivo}.")]
    private static partial void RegistrarFalha(ILogger logger, string motivo);
}
