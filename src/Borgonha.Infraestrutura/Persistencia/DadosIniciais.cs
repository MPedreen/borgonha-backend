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
        AdicionarCookie(cookies, "tradicional", "Tradicional", 0, "100g",
            "Massa de manteiga tostada e gotas de chocolate meio amargo, finalizado com flor de sal para realçar os sabores e trazer o equilíbrio perfeito entre doce e salgado.",
            "Manteiga tostada e flor de sal: o clássico que nunca erra.",
            "Cookie tradicional visto de cima, com pedaços de chocolate e flor de sal");
        AdicionarCookie(cookies, "nutella", "Nutella", 1, "130g",
            "Massa tradicional com gotas de chocolate meio amargo e um recheio generoso de Nutella. Porque os clássicos são clássicos...",
            "Recheado de Nutella. Precisa dizer mais?",
            "Cookie de Nutella visto de cima, com fios de chocolate",
            "Cookie de Nutella partido ao meio, com a Nutella escorrendo");
        AdicionarCookie(cookies, "bueno", "Kinder Bueno", 2, "130g",
            "Massa tradicional com gotas de chocolate branco e chocolate ao leite, recheada com o nosso maravilhoso creme de Kinder Bueno.",
            "O sabor do Kinder Bueno, agora em versão cookie.",
            "Cookie de Kinder Bueno visto de cima, com um pedaço de Kinder Bueno",
            "Cookie de Kinder Bueno partido ao meio, com recheio cremoso e um pedaço de Kinder Bueno por cima");
        AdicionarCookie(cookies, "triplo-chocolate", "Triplo Chocolate", 3, "130g",
            "Nosso Triplo Chocolate é para os verdadeiros amantes de chocolate! Uma irresistível massa de cacau black com três tipos de chocolate: ao leite, branco e meio amargo, finalizada com um delicioso recheio cremoso de Galak.",
            "Para quem acha que chocolate nunca é demais.",
            "Cookie triplo chocolate de massa escura visto de cima, com pedaços de chocolate branco",
            "Cookie triplo chocolate partido ao meio, com recheio cremoso de Galak");
        AdicionarCookie(cookies, "red-velvet", "Red Velvet", 4, "130g",
            "Massa com chocolate branco e recheio de Galak com cream cheese. Uma combinação equilibrada entre o sabor intenso do chocolate branco e o sabor do cream cheese.",
            "Vermelho por fora, cremoso por dentro.",
            "Cookie red velvet visto de cima, com gotas de chocolate branco",
            "Cookie red velvet partido ao meio, mostrando o recheio de Galak com cream cheese");
        AdicionarCookie(cookies, "mms", "M&M", 5, "115g",
            "Massa tradicional com gotas de chocolate ao leite e M&M’s, recheada com chocolate meio amargo para uma combinação irresistível de sabores e texturas.",
            "Colorido por fora, chocolate derretido por dentro.",
            "Cookie de M&M’s visto de cima, com confeitos coloridos",
            "Cookie de M&M’s partido ao meio, com recheio de chocolate");

        var doces = Categoria.Criar(Slug.Criar("doces"), "Doces", 1);
        doces.AdicionarProduto(Produto.Criar(
            Slug.Criar("brookie"),
            "Brookie",
            "Nosso famoso Brookie! A combinação perfeita entre nosso cookie artesanal e brownie artesanal, com uma generosa camada de Nutella. Cremoso por dentro e muito chocolatudo!",
            0,
            frase: "Brownie + cookie: o melhor dos dois mundos.",
            peso: "170g",
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
        string peso,
        string descricao,
        string frase,
        string textoAlternativoInteiro,
        string? textoAlternativoPartido = null)
    {
        // Foto do cookie Tradicional partido: pendente.
        var imagemPartido = textoAlternativoPartido is null
            ? null
            : new Imagem($"cookies/{slug}-partido", textoAlternativoPartido);

        categoria.AdicionarProduto(Produto.Criar(
            Slug.Criar(slug),
            nome,
            descricao,
            ordem,
            frase,
            peso,
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
