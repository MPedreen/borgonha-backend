using Borgonha.Dominio.Lojas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Borgonha.Infraestrutura.Persistencia.Configuracoes;

internal sealed class LojaConfiguracao : IEntityTypeConfiguration<Loja>
{
    private const int TamanhoMaximoDiaDaSemana = 10;
    private const int TamanhoMaximoTipoCanal = 20;

    public void Configure(EntityTypeBuilder<Loja> builder)
    {
        builder.ToTable("lojas");
        builder.HasKey(loja => loja.Id);

        builder.Property(loja => loja.Nome).HasMaxLength(Loja.TamanhoMaximoNome);
        builder.Property(loja => loja.Telefone).HasMaxLength(Loja.TamanhoMaximoTelefone);
        builder.Property(loja => loja.Instagram).HasMaxLength(Loja.TamanhoMaximoInstagram);
        builder.Property(loja => loja.AvisoTopo).HasMaxLength(Loja.TamanhoMaximoAvisoTopo);

        builder.OwnsOne(loja => loja.Endereco, endereco =>
        {
            endereco.Property(e => e.Logradouro).HasColumnName("logradouro").HasMaxLength(Endereco.TamanhoMaximoLogradouro);
            endereco.Property(e => e.Numero).HasColumnName("numero").HasMaxLength(Endereco.TamanhoMaximoNumero);
            endereco.Property(e => e.Bairro).HasColumnName("bairro").HasMaxLength(Endereco.TamanhoMaximoBairro);
            endereco.Property(e => e.Cidade).HasColumnName("cidade").HasMaxLength(Endereco.TamanhoMaximoCidade);
            endereco.Property(e => e.Uf).HasColumnName("uf").HasMaxLength(2);
            endereco.Property(e => e.Cep).HasColumnName("cep").HasMaxLength(9);
        });
        builder.Navigation(loja => loja.Endereco).IsRequired();

        builder.OwnsOne(loja => loja.Coordenadas, coordenadas =>
        {
            coordenadas.Property(c => c.Latitude).HasColumnName("latitude");
            coordenadas.Property(c => c.Longitude).HasColumnName("longitude");
        });
        builder.Navigation(loja => loja.Coordenadas).IsRequired();

        builder.OwnsOne(loja => loja.Entrega, entrega =>
        {
            entrega.Property(e => e.RaioSemPedidoMinimoEmMetros);
            entrega.Property(e => e.PedidoMinimo).HasPrecision(10, 2);
        });

        builder.OwnsMany(loja => loja.Horarios, horario =>
        {
            horario.ToTable("horarios_funcionamento");
            horario.WithOwner().HasForeignKey("LojaId");
            horario.Property<int>("Id");
            horario.HasKey("Id");
            horario.Property(h => h.DiaInicial).HasConversion<string>().HasMaxLength(TamanhoMaximoDiaDaSemana);
            horario.Property(h => h.DiaFinal).HasConversion<string>().HasMaxLength(TamanhoMaximoDiaDaSemana);
        });

        builder.OwnsMany(loja => loja.CanaisPedido, canal =>
        {
            canal.ToTable("canais_pedido");
            canal.WithOwner().HasForeignKey("LojaId");
            canal.Property<int>("Id");
            canal.HasKey("Id");
            canal.Property(c => c.Tipo).HasConversion<string>().HasMaxLength(TamanhoMaximoTipoCanal);
            canal.Property(c => c.Titulo).HasMaxLength(CanalPedido.TamanhoMaximoTitulo);
            canal.Property(c => c.Descricao).HasMaxLength(CanalPedido.TamanhoMaximoDescricao);
            canal.Property(c => c.Url).HasMaxLength(CanalPedido.TamanhoMaximoUrl);
            canal.Property(c => c.MensagemPadrao).HasMaxLength(CanalPedido.TamanhoMaximoMensagemPadrao);
            canal.Property(c => c.Selo).HasMaxLength(CanalPedido.TamanhoMaximoSelo);
        });
    }
}
