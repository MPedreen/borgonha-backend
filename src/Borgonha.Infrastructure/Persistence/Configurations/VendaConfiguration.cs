using Borgonha.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Borgonha.Infrastructure.Persistence.Configurations;

internal sealed class VendaConfiguration : IEntityTypeConfiguration<Venda>
{
    public void Configure(EntityTypeBuilder<Venda> builder)
    {
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).ValueGeneratedNever();

        builder.Property(v => v.CriadoPor)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(v => v.Total)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(v => v.ValorPago)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(v => v.Troco)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(v => v.DataHora)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.HasMany(v => v.Itens)
            .WithOne()
            .HasForeignKey(i => i.VendaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(v => v.Itens)
            .HasField("_itens")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
