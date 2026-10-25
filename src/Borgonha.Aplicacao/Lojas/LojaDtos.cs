namespace Borgonha.Aplicacao.Lojas;

public sealed record LojaDto(
    string Nome,
    EnderecoDto Endereco,
    CoordenadasDto Coordenadas,
    string Telefone,
    string? Instagram,
    string? AvisoTopo,
    EntregaDto? Entrega,
    IReadOnlyList<HorarioDto> Horarios,
    IReadOnlyList<CanalPedidoDto> CanaisPedido);

public sealed record EnderecoDto(string Logradouro, string Numero, string Bairro, string Cidade, string Uf, string? Cep);

public sealed record CoordenadasDto(double Latitude, double Longitude);

/// <param name="RaioSemPedidoMinimoEmMetros">Até essa distância da loja, em linha reta, não há pedido mínimo.</param>
/// <param name="PedidoMinimo">Valor mínimo do pedido, em reais, para endereços mais distantes.</param>
public sealed record EntregaDto(int RaioSemPedidoMinimoEmMetros, decimal PedidoMinimo);

/// <param name="Dias">Dias da semana em inglês e minúsculas ("tuesday"), o formato usado pelo schema.org.</param>
/// <param name="Abertura">Horário no formato HH:mm.</param>
/// <param name="Fechamento">Horário no formato HH:mm.</param>
public sealed record HorarioDto(IReadOnlyList<string> Dias, string Abertura, string Fechamento);

/// <param name="Tipo">"whatsapp", "ifood" ou "loja-fisica".</param>
public sealed record CanalPedidoDto(
    string Tipo,
    string Titulo,
    string Descricao,
    Uri Url,
    string? MensagemPadrao,
    string? Selo,
    bool Destaque);
