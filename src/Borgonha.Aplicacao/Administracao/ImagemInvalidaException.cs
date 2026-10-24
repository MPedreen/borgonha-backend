namespace Borgonha.Aplicacao.Administracao;

/// <summary>
/// O arquivo enviado não é uma foto aceita (formato, tamanho ou dimensões).
/// </summary>
public sealed class ImagemInvalidaException : Exception
{
    public ImagemInvalidaException()
    {
    }

    public ImagemInvalidaException(string message)
        : base(message)
    {
    }

    public ImagemInvalidaException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
