using Borgonha.Domain.Entities;
using Borgonha.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Borgonha.Infrastructure.Persistence.Configurations;

internal sealed class MovimentacaoEstoqueConfiguration : IEntityTypeConfiguration<MovimentacaoEstoque>
{
    public void Configure(EntityTypeBuilder<MovimentacaoEstoque> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedNever();

        builder.Property(m => m.Tipo)
            .HasConversion(
                tipo => tipo == TipoMovimentacao.Entrada ? "entrada" : "saida",
                valor => valor == "entrada" ? TipoMovimentacao.Entrada : TipoMovimentacao.Saida)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(m => m.Quantidade)
            .HasPrecision(10, 3)
            .IsRequired();

        builder.Property(m => m.DataHora)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(m => m.Observacao)
            .HasColumnType("text");

        builder.HasOne<Venda>()
            .WithMany()
            .HasForeignKey(m => m.VendaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(tb => tb.HasCheckConstraint(
            "ck_movimentacoes_estoque_tipo",
            "tipo IN ('entrada', 'saida')"));
    }
}
