namespace Borgonha.Dominio.Comum;

/// <summary>
/// Indica que uma regra de negócio foi violada.
/// </summary>
public sealed class DominioException : Exception
{
    public DominioException()
    {
    }

    public DominioException(string message)
        : base(message)
    {
    }

    public DominioException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
