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

    // Cardápio e preços enviados pela Borgonha em 24/10/2026. Os produtos fora do cardápio de hoje
    // entram ocultos: voltam ao site com um clique na área do administrador.
    private static IEnumerable<Categoria> CriarCategorias()
    {
        var cookies = Categoria.Criar(Slug.Criar("cookies"), "Cookies", 0);
        AdicionarCookie(cookies, "tradicional", "Tradicional", 0, "100g", 10m,
            "Massa de manteiga tostada e gotas de chocolate meio amargo, finalizado com flor de sal para realçar os sabores e trazer o equilíbrio perfeito entre doce e salgado.",
            "Manteiga tostada e flor de sal: o clássico que nunca erra.",
            "Cookie tradicional visto de cima, com pedaços de chocolate e flor de sal");
        AdicionarCookie(cookies, "nutella", "Nutella", 1, "130g", 15m,
            "Massa tradicional com gotas de chocolate meio amargo e um recheio generoso de Nutella. Porque os clássicos são clássicos...",
            "Recheado de Nutella. Precisa dizer mais?",
            "Cookie de Nutella visto de cima, com fios de chocolate",
            "Cookie de Nutella partido ao meio, com a Nutella escorrendo");
        AdicionarCookie(cookies, "bueno", "Kinder Bueno", 2, "130g", 15m,
            "Massa tradicional com gotas de chocolate branco e chocolate ao leite, recheada com o nosso maravilhoso creme de Kinder Bueno.",
            "O sabor do Kinder Bueno, agora em versão cookie.",
            "Cookie de Kinder Bueno visto de cima, com um pedaço de Kinder Bueno",
            "Cookie de Kinder Bueno partido ao meio, com recheio cremoso e um pedaço de Kinder Bueno por cima");
        AdicionarCookie(cookies, "triplo-chocolate", "Triplo Chocolate", 3, "130g", 15m,
            "Nosso Triplo Chocolate é para os verdadeiros amantes de chocolate! Uma irresistível massa de cacau black com três tipos de chocolate: ao leite, branco e meio amargo, finalizada com um delicioso recheio cremoso de Galak.",
            "Para quem acha que chocolate nunca é demais.",
            "Cookie triplo chocolate de massa escura visto de cima, com pedaços de chocolate branco",
            "Cookie triplo chocolate partido ao meio, com recheio cremoso de Galak");
        AdicionarCookie(cookies, "red-velvet", "Red Velvet", 4, "130g", 15m,
            "Massa com chocolate branco e recheio de Galak com cream cheese. Uma combinação equilibrada entre o sabor intenso do chocolate branco e o sabor do cream cheese.",
            "Vermelho por fora, cremoso por dentro.",
            "Cookie red velvet visto de cima, com gotas de chocolate branco",
            "Cookie red velvet partido ao meio, mostrando o recheio de Galak com cream cheese");
        AdicionarCookie(cookies, "mms", "M&M", 5, "115g", 12m,
            "Massa tradicional com gotas de chocolate ao leite e M&M’s, recheada com chocolate meio amargo para uma combinação irresistível de sabores e texturas.",
            "Colorido por fora, chocolate derretido por dentro.",
            "Cookie de M&M’s visto de cima, com confeitos coloridos",
            "Cookie de M&M’s partido ao meio, com recheio de chocolate");

        // Os doces novos ainda não têm descrição nem foto: entram pela área do administrador.
        var doces = Categoria.Criar(Slug.Criar("doces"), "Doces", 1);
        Adicionar(doces, 15m, Produto.Criar(
            Slug.Criar("brookie"),
            "Brookie",
            "Nosso famoso Brookie! A combinação perfeita entre nosso cookie artesanal e brownie artesanal, com uma generosa camada de Nutella. Cremoso por dentro e muito chocolatudo!",
            0,
            frase: "Brownie + cookie: o melhor dos dois mundos.",
            peso: "170g",
            imagemInteiro: new Imagem("doces/brookie", "Brookie na marmitinha, com camada de cookie e Nutella por cima")));
        Adicionar(doces, 12m, Produto.Criar(Slug.Criar("brownie-ninho-com-nutella"), "Brownie Ninho com Nutella", null, 1));
        Adicionar(doces, 9m, Produto.Criar(
            Slug.Criar("brownie-doce-de-leite"),
            "Brownie de doce de leite",
            "Brownie com camada de doce de leite, embalado individualmente.",
            2,
            imagemInteiro: new Imagem("doces/brownie-doce-de-leite", "Brownies de doce de leite embalados com o adesivo da Borgonha")));
        Adicionar(doces, 12m, Produto.Criar(Slug.Criar("nuvem-de-abacaxi"), "Nuvem de abacaxi", null, 3));
        Adicionar(doces, 15m, Produto.Criar(Slug.Criar("torta-chocolate-com-maracuja-cremoso"), "Torta chocolate com maracujá cremoso", null, 4));
        Adicionar(doces, 15m, Produto.Criar(Slug.Criar("copo-da-felicidade-ninho-com-nutella"), "Copo da felicidade Ninho com Nutella", null, 5));
        Adicionar(doces, 19m, Produto.Criar(Slug.Criar("matilda"), "Matilda", null, 6));
        Adicionar(doces, 16m, Produto.Criar(Slug.Criar("bolo-crocante"), "Bolo crocante", null, 7));
        Adicionar(doces, 13m, Produto.Criar(Slug.Criar("palha-ninho-com-nutella"), "Palha Ninho com Nutella", null, 8));
        Adicionar(doces, 13m, Produto.Criar(Slug.Criar("palha-biscoff"), "Palha Biscoff", null, 9));
        Adicionar(doces, null, oculto: true, produto: Produto.Criar(
            Slug.Criar("bolo-de-pote"),
            "Bolo de pote",
            "Camadas de bolo e recheio cremoso, no pote.",
            10,
            variacoes: ["Chocolate & Maracujá", "Frutas Vermelhas & Ninho", "Nutella & Ninho"],
            imagemInteiro: new Imagem("doces/bolo-de-pote", "Bolo de pote de Nutella com Ninho, com camadas de bolo e recheio")));

        // Fotos dos salgados: pendentes.
        var salgados = Categoria.Criar(Slug.Criar("salgados"), "Salgados", 2);
        Adicionar(salgados, 10m, Produto.Criar(
            Slug.Criar("coxinha-frango-catupiry"),
            "Coxinha de frango com catupiry",
            "Coxinha recheada com frango e catupiry.",
            0));
        Adicionar(salgados, 10m, Produto.Criar(Slug.Criar("bolinho-de-aipim"), "Bolinho de aipim", null, 1));
        Adicionar(salgados, null, oculto: true, produto: Produto.Criar(
            Slug.Criar("empadao-frango"),
            "Empadão de frango",
            "Empadão de frango servido na marmitinha.",
            2));

        return [cookies, doces, salgados];
    }

    private static void AdicionarCookie(
        Categoria categoria,
        string slug,
        string nome,
        int ordem,
        string peso,
        decimal preco,
        string descricao,
        string frase,
        string textoAlternativoInteiro,
        string? textoAlternativoPartido = null)
    {
        // Foto do cookie Tradicional partido: pendente.
        var imagemPartido = textoAlternativoPartido is null
            ? null
            : new Imagem($"cookies/{slug}-partido", textoAlternativoPartido);

        Adicionar(categoria, preco, Produto.Criar(
            Slug.Criar(slug),
            nome,
            descricao,
            ordem,
            frase,
            peso,
            imagemInteiro: new Imagem($"cookies/{slug}-inteiro", textoAlternativoInteiro),
            imagemPartido: imagemPartido));
    }

    private static void Adicionar(Categoria categoria, decimal? preco, Produto produto, bool oculto = false)
    {
        produto.DefinirPreco(preco);
        if (oculto)
        {
            produto.DefinirSituacao(SituacaoProduto.Oculto);
        }

        categoria.AdicionarProduto(produto);
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
