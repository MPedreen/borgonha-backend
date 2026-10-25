using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Borgonha.Infraestrutura.Persistencia;

/// <summary>
/// Usada apenas pelas ferramentas do EF Core (dotnet ef migrations ...). Nenhuma conexão é aberta:
/// o servidor só precisa ser o PostgreSQL para as migrations saírem com os tipos dele.
/// </summary>
public sealed class FabricaDbContextDesignTime : IDesignTimeDbContextFactory<BorgonhaDbContext>
{
    public BorgonhaDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<BorgonhaDbContext>()
            .UseNpgsql("Host=localhost;Database=borgonha-design")
            .UseSnakeCaseNamingConvention()
            .Options);
}
