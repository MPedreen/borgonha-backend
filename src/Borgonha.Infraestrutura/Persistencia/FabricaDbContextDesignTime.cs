using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Borgonha.Infraestrutura.Persistencia;

/// <summary>
/// Usada apenas pelas ferramentas do EF Core (dotnet ef migrations ...).
/// </summary>
public sealed class FabricaDbContextDesignTime : IDesignTimeDbContextFactory<BorgonhaDbContext>
{
    public BorgonhaDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<BorgonhaDbContext>().UseSqlite("Data Source=borgonha-design.db").Options);
}
