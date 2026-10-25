namespace Borgonha.Aplicacao.Entrega;

/// <summary>
/// O serviço de mapas não pôde responder: sem chave, limite diário de uso atingido, fora do ar ou lento demais.
/// O site pede ao cliente que confirme o endereço pelo WhatsApp.
/// </summary>
public sealed class MapasIndisponiveisException : Exception
{
    public MapasIndisponiveisException()
        : base("O serviço de mapas está indisponível.")
    {
    }

    public MapasIndisponiveisException(string message)
        : base(message)
    {
    }

    public MapasIndisponiveisException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
