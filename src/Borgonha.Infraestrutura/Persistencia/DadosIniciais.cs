using Borgonha.Dominio.Catalogo;
using Borgonha.Dominio.Comum;
using Borgonha.Dominio.Duvidas;
using Borgonha.Dominio.Lojas;
using Microsoft.EntityFrameworkCore;

namespace Borgonha.Infraestrutura.Persistencia;

/// <summary>
/// Cardápio, dados da loja e dúvidas cadastrados na primeira execução (banco vazio).
/// Fotos que ainda não existem ficam sem imagem e o site mostra um espaço reservado.
/// </summary>
internal static class DadosIniciais
{
    public static void Semear(DbContext contexto, bool _)
    {
        if (!contexto.Set<Categoria>().Any())
        {
            contexto.AddRange(CriarCategorias());
        }

        if (!contexto.Set<Loja>().Any())
        {
            contexto.Add(CriarLoja());
        }

        if (!contexto.Set<Duvida>().Any())
        {
            contexto.AddRange(CriarDuvidas());
        }

        contexto.SaveChanges();
    }

    public static async Task SemearAsync(DbContext contexto, bool _, CancellationToken cancellationToken)
    {
        if (!await contexto.Set<Categoria>().AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            contexto.AddRange(CriarCategorias());
        }

        if (!await contexto.Set<Loja>().AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            contexto.Add(CriarLoja());
        }

        if (!await contexto.Set<Duvida>().AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            contexto.AddRange(CriarDuvidas());
        }

        await contexto.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static IEnumerable<Categoria> CriarCategorias()
    {
        var cookies = Categoria.Criar(Slug.Criar("cookies"), "Cookies", 0);
        AdicionarCookie(cookies, "tradicional", "Tradicional", 0,
            "Massa com manteiga tostada, gotas de chocolate meio amargo e flor de sal.",
            "Manteiga tostada e flor de sal: o clássico que nunca erra.",
            "Cookie tradicional visto de cima, com pedaços de chocolate e flor de sal");
        AdicionarCookie(cookies, "nutella", "Nutella", 1,
            "Massa de baunilha, gotas de chocolate meio amargo e recheio de Nutella.",
            "Recheado de Nutella. Precisa dizer mais?",
            "Cookie de Nutella visto de cima, com fios de chocolate",
            "Cookie de Nutella partido ao meio, com a Nutella escorrendo");
        AdicionarCookie(cookies, "bueno", "Bueno", 2,
            "Massa de baunilha, gotas de chocolate branco e meio amargo e recheio Bueno.",
            "O sabor do Bueno, agora em versão cookie.",
            "Cookie Bueno visto de cima, com um pedaço de Kinder Bueno",
            "Cookie Bueno partido ao meio, com recheio cremoso e um pedaço de Kinder Bueno por cima");
        AdicionarCookie(cookies, "triplo-chocolate", "Triplo Chocolate", 3,
            "Massa de cacau black, gotas de chocolate branco, blend e meio amargo e recheio Galak.",
            "Para quem acha que chocolate nunca é demais.",
            "Cookie triplo chocolate de massa escura visto de cima, com pedaços de chocolate branco",
            "Cookie triplo chocolate partido ao meio, com recheio branco cremoso");
        AdicionarCookie(cookies, "red-velvet", "Red Velvet", 4,
            "Massa red velvet, gotas de chocolate branco e recheio de brigadeiro de Ninho com cream cheese.",
            "Vermelho por fora, cremoso por dentro.",
            "Cookie red velvet visto de cima, com gotas de chocolate branco",
            "Cookie red velvet partido ao meio, mostrando o recheio de brigadeiro de Ninho com cream cheese");
        AdicionarCookie(cookies, "pistache", "Pistache", 5,
            "Massa de baunilha, gotas de chocolate branco e recheio de creme de pistache.",
            "Verdinho, cremoso e difícil de dividir.",
            "Cookie de pistache visto de cima, com fios de creme de pistache e pistache picado");
        AdicionarCookie(cookies, "bala-baiana", "Bala Baiana", 6,
            "Massa com manteiga tostada, gotas de chocolate branco, coco, bites de caramelo e amêndoas laminadas.",
            "Coco, caramelo e amêndoas: a bala baiana virou cookie.",
            "Cookie bala baiana visto de cima, com coco e amêndoas laminadas");
        AdicionarCookie(cookies, "mms", "M&M", 7,
            "Massa de baunilha, M&M's e chocolate meio amargo, recheado com chocolate meio amargo.",
            "Colorido por fora, chocolate derretido por dentro.",
            "Cookie de M&M's visto de cima, com confeitos coloridos",
            "Cookie de M&M's partido ao meio, com recheio de chocolate");

        var doces = Categoria.Criar(Slug.Criar("doces"), "Doces", 1);
        doces.AdicionarProduto(Produto.Criar(
            Slug.Criar("brookie"),
            "Brookie",
            "Brownie e cookie na mesma marmitinha, finalizados com Nutella.",
            0,
            frase: "Brownie + cookie: o melhor dos dois mundos.",
            imagemInteiro: new Imagem("doces/brookie", "Brookie na marmitinha, com camada de cookie e Nutella por cima")));
        doces.AdicionarProduto(Produto.Criar(
            Slug.Criar("brownie-doce-de-leite"),
            "Brownie de doce de leite",
            "Brownie com camada de doce de leite, embalado individualmente.",
            1,
            imagemInteiro: new Imagem("doces/brownie-doce-de-leite", "Brownies de doce de leite embalados com o adesivo da Borgonha")));
        doces.AdicionarProduto(Produto.Criar(
            Slug.Criar("bolo-de-pote"),
            "Bolo de pote",
            "Camadas de bolo e recheio cremoso, no pote.",
            2,
            variacoes: ["Chocolate & Maracujá", "Frutas Vermelhas & Ninho", "Nutella & Ninho"],
            imagemInteiro: new Imagem("doces/bolo-de-pote", "Bolo de pote de Nutella com Ninho, com camadas de bolo e recheio")));

        // Fotos dos salgados: pendentes.
        var salgados = Categoria.Criar(Slug.Criar("salgados"), "Salgados", 2);
        salgados.AdicionarProduto(Produto.Criar(
            Slug.Criar("coxinha-frango-catupiry"),
            "Coxinha de frango com catupiry",
            "Coxinha recheada com frango e catupiry.",
            0));
        salgados.AdicionarProduto(Produto.Criar(
            Slug.Criar("empadao-frango"),
            "Empadão de frango",
            "Empadão de frango servido na marmitinha.",
            1));

        return [cookies, doces, salgados];
    }

    private static void AdicionarCookie(
        Categoria categoria,
        string slug,
        string nome,
        int ordem,
        string descricao,
        string frase,
        string textoAlternativoInteiro,
        string? textoAlternativoPartido = null)
    {
        // Fotos dos cookies partidos de Tradicional, Pistache e Bala Baiana: pendentes.
        var imagemPartido = textoAlternativoPartido is null
            ? null
            : new Imagem($"cookies/{slug}-partido", textoAlternativoPartido);

        categoria.AdicionarProduto(Produto.Criar(
            Slug.Criar(slug),
            nome,
            descricao,
            ordem,
            frase,
            imagemInteiro: new Imagem($"cookies/{slug}-inteiro", textoAlternativoInteiro),
            imagemPartido: imagemPartido));
    }

    private static Loja CriarLoja()
    {
        var loja = Loja.Criar(
            "Borgonha Confeitaria",
            new Endereco("Rua Afrânio Peixoto", "40", "Santa Luzia", "São Gonçalo", "RJ"),
            new Coordenadas(-22.8021494, -42.9803336),
            "+5521974509789",
            instagram: "borgonhaconfeitaria",
            avisoTopo: "Frete grátis em Santa Luzia e região pedindo pelo WhatsApp");

        loja.AdicionarHorario(new HorarioFuncionamento(DayOfWeek.Tuesday, DayOfWeek.Friday, new TimeOnly(11, 0), new TimeOnly(19, 0)));
        loja.AdicionarHorario(new HorarioFuncionamento(DayOfWeek.Saturday, DayOfWeek.Saturday, new TimeOnly(11, 0), new TimeOnly(16, 0)));

        loja.AdicionarCanalPedido(new CanalPedido(
            TipoCanalPedido.WhatsApp,
            "Pedir pelo WhatsApp",
            "MELHORES PREÇOS, entrega exclusiva em Santa Luzia e região com frete grátis!",
            new Uri("https://wa.me/5521974509789"),
            0,
            mensagemPadrao: "Olá!\nDesejo fazer meu pedido!",
            destaque: true));
        loja.AdicionarCanalPedido(new CanalPedido(
            TipoCanalPedido.IFood,
            "Pedir pelo iFood",
            "Peça pelo app do iFood.",
            new Uri("https://www.ifood.com.br/delivery/sao-goncalo-rj/borgonha-confeitaria-santa-luzia/b5d4336d-e32f-4558-b5f7-10ca4697ace8?UTM_Medium=share"),
            1));
        loja.AdicionarCanalPedido(new CanalPedido(
            TipoCanalPedido.LojaFisica,
            "Encontre nossa loja física",
            "Rua Afrânio Peixoto, 40 – Santa Luzia, São Gonçalo – RJ",
            new Uri("https://maps.app.goo.gl/etqznn6CLoQ8DE68A"),
            2));

        return loja;
    }

    private static IEnumerable<Duvida> CriarDuvidas() =>
    [
        Duvida.Criar(
            "Como faço meu pedido?",
            "Pelo WhatsApp, com os melhores preços e frete grátis em Santa Luzia e região, pelo iFood ou direto na nossa loja física. É só tocar em “Faça seu pedido”.",
            0),
        Duvida.Criar(
            "O frete é grátis?",
            "Sim! Pedindo pelo WhatsApp, a entrega é grátis em Santa Luzia e região.",
            1),
        Duvida.Criar(
            "Qual o horário de funcionamento?",
            "De terça a sexta, das 11h às 19h, e aos sábados, das 11h às 16h.",
            2),
        Duvida.Criar(
            "Onde fica a loja?",
            "Na Rua Afrânio Peixoto, 40 – Santa Luzia, São Gonçalo – RJ.",
            3),
    ];
}
