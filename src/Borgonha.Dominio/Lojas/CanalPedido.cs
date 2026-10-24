using Borgonha.Dominio.Comum;

namespace Borgonha.Dominio.Lojas;

/// <summary>
/// Forma de fazer o pedido exibida no botão "Faça seu pedido" (WhatsApp, iFood ou loja física).
/// </summary>
public sealed class CanalPedido
{
    public const int TamanhoMaximoTitulo = 60;
    public const int TamanhoMaximoDescricao = 160;
    public const int TamanhoMaximoMensagemPadrao = 300;
    public const int TamanhoMaximoSelo = 24;
    public const int TamanhoMaximoUrl = 500;

    public CanalPedido(
        TipoCanalPedido tipo,
        string titulo,
        string descricao,
        Uri url,
        int ordem,
        string? mensagemPadrao = null,
        string? selo = null,
        bool destaque = false)
    {
        if (!Enum.IsDefined(tipo))
        {
            throw new DominioException("Tipo de canal de pedido inválido.");
        }

        mensagemPadrao = Garantir.TextoOpcional(mensagemPadrao, "Mensagem padrão", TamanhoMaximoMensagemPadrao);
        if (mensagemPadrao is not null && tipo != TipoCanalPedido.WhatsApp)
        {
            throw new DominioException("A mensagem padrão só se aplica ao WhatsApp.");
        }

        Tipo = tipo;
        Titulo = Garantir.TextoObrigatorio(titulo, "Título do canal", TamanhoMaximoTitulo);
        Descricao = Garantir.TextoObrigatorio(descricao, "Descrição do canal", TamanhoMaximoDescricao);
        Url = Garantir.LinkHttps(url, "Link do canal");
        Ordem = Garantir.NaoNegativo(ordem, "Ordem do canal");
        MensagemPadrao = mensagemPadrao;
        Selo = Garantir.TextoOpcional(selo, "Selo do canal", TamanhoMaximoSelo);
        Destaque = destaque;
    }

    public TipoCanalPedido Tipo { get; private set; }

    public string Titulo { get; private set; }

    public string Descricao { get; private set; }

    public Uri Url { get; private set; }

    public int Ordem { get; private set; }

    /// <summary>
    /// Texto que já vem escrito ao abrir a conversa do WhatsApp.
    /// </summary>
    public string? MensagemPadrao { get; private set; }

    public string? Selo { get; private set; }

    public bool Destaque { get; private set; }
}
