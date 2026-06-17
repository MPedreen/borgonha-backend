using Borgonha.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Borgonha.Infrastructure.Persistence.Configurations;

internal sealed class ReceitaItemConfiguration : IEntityTypeConfiguration<ReceitaItem>
{
    public void Configure(EntityTypeBuilder<ReceitaItem> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();

        builder.Property(r => r.Quantidade)
            .HasPrecision(10, 3)
            .IsRequired();

        builder.HasOne<Ingrediente>()
            .WithMany()
            .HasForeignKey(r => r.IngredienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => new { r.ProdutoId, r.IngredienteId })
            .IsUnique();
    }
}
