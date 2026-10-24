using Borgonha.Dominio.Duvidas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Borgonha.Infraestrutura.Persistencia.Configuracoes;

internal sealed class DuvidaConfiguracao : IEntityTypeConfiguration<Duvida>
{
    public void Configure(EntityTypeBuilder<Duvida> builder)
    {
        builder.ToTable("Duvidas");
        builder.HasKey(duvida => duvida.Id);

        builder.Property(duvida => duvida.Pergunta).HasMaxLength(Duvida.TamanhoMaximoPergunta);
        builder.Property(duvida => duvida.Resposta).HasMaxLength(Duvida.TamanhoMaximoResposta);
    }
}
