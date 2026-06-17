using Borgonha.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Borgonha.Infrastructure.Persistence.Configurations;

internal sealed class ProdutoConfiguration : IEntityTypeConfiguration<Produto>
{
    public void Configure(EntityTypeBuilder<Produto> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Nome)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.PrecoVenda)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(p => p.Ativo)
            .IsRequired();

        builder.Property(p => p.CriadoEm)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Ignore(p => p.PossuiReceita);

        builder.HasMany(p => p.Receita)
            .WithOne()
            .HasForeignKey(r => r.ProdutoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.Receita)
            .HasField("_receita")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
