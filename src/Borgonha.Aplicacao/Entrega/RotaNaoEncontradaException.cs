namespace Borgonha.Aplicacao.Entrega;

/// <summary>
/// Não existe trajeto entre a loja e o ponto pedido (por exemplo, um ponto longe de qualquer rua).
/// </summary>
public sealed class RotaNaoEncontradaException : Exception
{
    public RotaNaoEncontradaException()
        : base("Não há trajeto até esse ponto.")
    {
    }

    public RotaNaoEncontradaException(string message)
        : base(message)
    {
    }

    public RotaNaoEncontradaException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
