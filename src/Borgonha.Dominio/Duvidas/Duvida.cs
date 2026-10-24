using Borgonha.Dominio.Comum;

namespace Borgonha.Dominio.Duvidas;

/// <summary>
/// Pergunta frequente exibida na seção de dúvidas.
/// </summary>
public sealed class Duvida
{
    public const int TamanhoMaximoPergunta = 160;
    public const int TamanhoMaximoResposta = 600;

    private Duvida()
    {
    }

    public int Id { get; private set; }

    public string Pergunta { get; private set; } = string.Empty;

    public string Resposta { get; private set; } = string.Empty;

    public int Ordem { get; private set; }

    public static Duvida Criar(string pergunta, string resposta, int ordem) =>
        new()
        {
            Pergunta = Garantir.TextoObrigatorio(pergunta, "Pergunta", TamanhoMaximoPergunta),
            Resposta = Garantir.TextoObrigatorio(resposta, "Resposta", TamanhoMaximoResposta),
            Ordem = Garantir.NaoNegativo(ordem, "Ordem da dúvida"),
        };
}
