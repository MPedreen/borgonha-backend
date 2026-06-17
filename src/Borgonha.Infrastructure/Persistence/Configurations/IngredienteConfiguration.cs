using Borgonha.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Borgonha.Infrastructure.Persistence.Configurations;

internal sealed class IngredienteConfiguration : IEntityTypeConfiguration<Ingrediente>
{
    public void Configure(EntityTypeBuilder<Ingrediente> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedNever();

        builder.Property(i => i.Nome)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(i => i.Unidade)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(i => i.QuantidadeAtual)
            .HasPrecision(10, 3)
            .IsRequired();

        builder.Property(i => i.QuantidadeMinima)
            .HasPrecision(10, 3)
            .IsRequired();

        builder.Property(i => i.CustoUnitario)
            .HasPrecision(10, 4)
            .IsRequired();

        builder.Property(i => i.CriadoEm)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Ignore(i => i.EstaEmAlerta);

        builder.HasMany(i => i.Movimentacoes)
            .WithOne()
            .HasForeignKey(m => m.IngredienteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(i => i.Movimentacoes)
            .HasField("_movimentacoes")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
