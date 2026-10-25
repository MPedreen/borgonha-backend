using Borgonha.Dominio.Catalogo;
using Borgonha.Dominio.Comum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Borgonha.Infraestrutura.Persistencia.Configuracoes;

internal sealed class ProdutoConfiguracao : IEntityTypeConfiguration<Produto>
{
    public void Configure(EntityTypeBuilder<Produto> builder)
    {
        builder.ToTable("produtos");
        builder.HasKey(produto => produto.Id);

        builder.Property(produto => produto.Slug).HasConversion<ConversorSlug>().HasMaxLength(Slug.TamanhoMaximo);
        builder.HasIndex(produto => produto.Slug).IsUnique();

        builder.Property(produto => produto.Nome).HasMaxLength(Produto.TamanhoMaximoNome);
        builder.Property(produto => produto.Descricao).HasMaxLength(Produto.TamanhoMaximoDescricao);
        builder.Property(produto => produto.Frase).HasMaxLength(Produto.TamanhoMaximoFrase);
        builder.Property(produto => produto.Preco).HasPrecision(10, 2);
        builder.Property(produto => produto.Peso).HasMaxLength(Produto.TamanhoMaximoPeso);
        builder.Property(produto => produto.Selo).HasMaxLength(Produto.TamanhoMaximoSelo);
        builder.PrimitiveCollection(produto => produto.Variacoes);
        builder.Ignore(produto => produto.Situacao);

        builder.OwnsOne(produto => produto.ImagemInteiro, imagem => ConfigurarImagem(imagem, "imagem_inteiro"));
        builder.OwnsOne(produto => produto.ImagemPartido, imagem => ConfigurarImagem(imagem, "imagem_partido"));

        builder.HasOne(produto => produto.Categoria)
            .WithMany(categoria => categoria.Produtos)
            .HasForeignKey(produto => produto.CategoriaId);
    }

    private static void ConfigurarImagem(OwnedNavigationBuilder<Produto, Imagem> imagem, string prefixoColuna)
    {
        imagem.Property(foto => foto.Caminho)
            .HasColumnName($"{prefixoColuna}_caminho")
            .HasMaxLength(Imagem.TamanhoMaximoCaminho);
        imagem.Property(foto => foto.TextoAlternativo)
            .HasColumnName($"{prefixoColuna}_texto_alternativo")
            .HasMaxLength(Imagem.TamanhoMaximoTextoAlternativo);
    }
}
