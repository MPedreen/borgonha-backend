using System.Globalization;
using Borgonha.Dominio.Lojas;

namespace Borgonha.Aplicacao.Lojas;

internal static class LojaMapeamento
{
    public static LojaDto ParaDto(this Loja loja) =>
        new(
            loja.Nome,
            new EnderecoDto(
                loja.Endereco.Logradouro,
                loja.Endereco.Numero,
                loja.Endereco.Bairro,
                loja.Endereco.Cidade,
                loja.Endereco.Uf,
                loja.Endereco.Cep),
            new CoordenadasDto(loja.Coordenadas.Latitude, loja.Coordenadas.Longitude),
            loja.Telefone,
            loja.Instagram,
            loja.AvisoTopo,
            [.. loja.Horarios.OrderBy(horario => horario.DiaInicial).Select(ParaDto)],
            [.. loja.CanaisPedido.OrderBy(canal => canal.Ordem).Select(ParaDto)]);

    private static HorarioDto ParaDto(HorarioFuncionamento horario) =>
        new(
            [.. horario.Dias().Select(dia => dia.ToString().ToLowerInvariant())],
            horario.Abertura.ToString("HH:mm", CultureInfo.InvariantCulture),
            horario.Fechamento.ToString("HH:mm", CultureInfo.InvariantCulture));

    private static CanalPedidoDto ParaDto(CanalPedido canal) =>
        new(
            ParaTexto(canal.Tipo),
            canal.Titulo,
            canal.Descricao,
            canal.Url,
            canal.MensagemPadrao,
            canal.Selo,
            canal.Destaque);

    private static string ParaTexto(TipoCanalPedido tipo) =>
        tipo switch
        {
            TipoCanalPedido.WhatsApp => "whatsapp",
            TipoCanalPedido.IFood => "ifood",
            TipoCanalPedido.LojaFisica => "loja-fisica",
            _ => throw new ArgumentOutOfRangeException(nameof(tipo), tipo, "Tipo de canal sem representação na API."),
        };
}
