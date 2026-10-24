using Borgonha.Dominio.Catalogo;
using Borgonha.Dominio.Comum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Borgonha.Infraestrutura.Persistencia.Configuracoes;

internal sealed class CategoriaConfiguracao : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        builder.ToTable("Categorias");
        builder.HasKey(categoria => categoria.Id);

        builder.Property(categoria => categoria.Slug).HasConversion<ConversorSlug>().HasMaxLength(Slug.TamanhoMaximo);
        builder.HasIndex(categoria => categoria.Slug).IsUnique();

        builder.Property(categoria => categoria.Nome).HasMaxLength(Categoria.TamanhoMaximoNome);
    }
}
