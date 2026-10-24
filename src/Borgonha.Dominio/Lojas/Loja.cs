using System.Text.RegularExpressions;
using Borgonha.Dominio.Comum;

namespace Borgonha.Dominio.Lojas;

/// <summary>
/// Dados da confeitaria exibidos no site: endereço, horários e canais de pedido.
/// </summary>
public sealed partial class Loja
{
    public const int TamanhoMaximoNome = 80;
    public const int TamanhoMaximoTelefone = 16;
    public const int TamanhoMaximoInstagram = 30;
    public const int TamanhoMaximoAvisoTopo = 140;

    private readonly List<HorarioFuncionamento> _horarios = [];
    private readonly List<CanalPedido> _canaisPedido = [];

    private Loja()
    {
    }

    public int Id { get; private set; }

    public string Nome { get; private set; } = string.Empty;

    public Endereco Endereco { get; private set; } = null!;

    public Coordenadas Coordenadas { get; private set; } = null!;

    /// <summary>
    /// Telefone no formato internacional E.164 (ex.: +5521974509789).
    /// </summary>
    public string Telefone { get; private set; } = string.Empty;

    /// <summary>
    /// Perfil do Instagram, sem o "@".
    /// </summary>
    public string? Instagram { get; private set; }

    /// <summary>
    /// Mensagem da barra de aviso no topo do site.
    /// </summary>
    public string? AvisoTopo { get; private set; }

    public IReadOnlyList<HorarioFuncionamento> Horarios => _horarios.AsReadOnly();

    public IReadOnlyList<CanalPedido> CanaisPedido => _canaisPedido.AsReadOnly();

    public static Loja Criar(
        string nome,
        Endereco endereco,
        Coordenadas coordenadas,
        string telefone,
        string? instagram = null,
        string? avisoTopo = null)
    {
        ArgumentNullException.ThrowIfNull(endereco);
        ArgumentNullException.ThrowIfNull(coordenadas);

        telefone = Garantir.TextoObrigatorio(telefone, "Telefone", TamanhoMaximoTelefone);
        if (!FormatoTelefone().IsMatch(telefone))
        {
            throw new DominioException("O telefone deve estar no formato internacional, por exemplo +5521999999999.");
        }

        instagram = Garantir.TextoOpcional(instagram, "Instagram", TamanhoMaximoInstagram);
        if (instagram is not null && !FormatoInstagram().IsMatch(instagram))
        {
            throw new DominioException("O perfil do Instagram deve conter só letras, números, ponto e sublinhado, sem o \"@\".");
        }

        return new Loja
        {
            Nome = Garantir.TextoObrigatorio(nome, "Nome da loja", TamanhoMaximoNome),
            Endereco = endereco,
            Coordenadas = coordenadas,
            Telefone = telefone,
            Instagram = instagram,
            AvisoTopo = Garantir.TextoOpcional(avisoTopo, "Aviso do topo", TamanhoMaximoAvisoTopo),
        };
    }

    public void AdicionarHorario(HorarioFuncionamento horario)
    {
        ArgumentNullException.ThrowIfNull(horario);

        if (_horarios.Exists(existente => existente.SobrepoeA(horario)))
        {
            throw new DominioException("Há dias repetidos nos horários de funcionamento.");
        }

        _horarios.Add(horario);
    }

    public void AdicionarCanalPedido(CanalPedido canal)
    {
        ArgumentNullException.ThrowIfNull(canal);

        if (_canaisPedido.Exists(existente => existente.Tipo == canal.Tipo))
        {
            throw new DominioException($"O canal de pedido '{canal.Tipo}' já foi cadastrado.");
        }

        _canaisPedido.Add(canal);
    }

    [GeneratedRegex(@"^\+[1-9]\d{7,14}$")]
    private static partial Regex FormatoTelefone();

    [GeneratedRegex(@"^[A-Za-z0-9._]{1,30}$")]
    private static partial Regex FormatoInstagram();
}
