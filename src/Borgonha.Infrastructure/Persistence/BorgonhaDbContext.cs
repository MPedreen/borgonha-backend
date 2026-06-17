using Borgonha.Domain.Entities;
using Borgonha.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Borgonha.Infrastructure.Persistence;

public sealed class BorgonhaDbContext : DbContext, IUnitOfWork
{
    public BorgonhaDbContext(DbContextOptions<BorgonhaDbContext> options) : base(options) { }

    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<ReceitaItem> ReceitaItens => Set<ReceitaItem>();
    public DbSet<Ingrediente> Ingredientes => Set<Ingrediente>();
    public DbSet<MovimentacaoEstoque> MovimentacoesEstoque => Set<MovimentacaoEstoque>();
    public DbSet<Venda> Vendas => Set<Venda>();
    public DbSet<ItemVenda> ItensVenda => Set<ItemVenda>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BorgonhaDbContext).Assembly);
    }
}
