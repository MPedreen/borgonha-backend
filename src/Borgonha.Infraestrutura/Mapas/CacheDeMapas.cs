using Microsoft.Extensions.Caching.Memory;

namespace Borgonha.Infraestrutura.Mapas;

/// <summary>
/// Guarda as respostas do serviço de mapas para que o mesmo endereço, digitado por qualquer pessoa, não
/// gaste crédito de novo. O tamanho é limitado, porque as chaves vêm do que o público digita.
/// </summary>
internal sealed class CacheDeMapas : IDisposable
{
    public const int Capacidade = 5_000;

    private readonly MemoryCache _cache = new(new MemoryCacheOptions { SizeLimit = Capacidade });

    public bool TentarObter<T>(string chave, out T? valor)
    {
        if (_cache.TryGetValue(chave, out var guardado) && guardado is T esperado)
        {
            valor = esperado;
            return true;
        }

        valor = default;
        return false;
    }

    public void Guardar<T>(string chave, T valor, TimeSpan duracao) =>
        _cache.Set(chave, valor, new MemoryCacheEntryOptions { Size = 1, AbsoluteExpirationRelativeToNow = duracao });

    public void Dispose() => _cache.Dispose();
}
