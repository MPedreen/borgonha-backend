using Borgonha.Dominio.Catalogo;
using Borgonha.Dominio.Duvidas;
using Borgonha.Dominio.Lojas;
using Microsoft.EntityFrameworkCore;

namespace Borgonha.Infraestrutura.Persistencia;

public sealed class BorgonhaDbContext(DbContextOptions<BorgonhaDbContext> options) : DbContext(options)
{
    public DbSet<Categoria> Categorias => Set<Categoria>();

    public DbSet<Produto> Produtos => Set<Produto>();

    public DbSet<Loja> Lojas => Set<Loja>();

    public DbSet<Duvida> Duvidas => Set<Duvida>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BorgonhaDbContext).Assembly);
    }
}
