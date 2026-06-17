using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Borgonha.Infrastructure.Persistence;

public sealed class BorgonhaDbContextFactory : IDesignTimeDbContextFactory<BorgonhaDbContext>
{
    public BorgonhaDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default")
            ?? "Host=localhost;Port=5432;Database=borgonha;Username=borgonha;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<BorgonhaDbContext>();
        optionsBuilder
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention();

        return new BorgonhaDbContext(optionsBuilder.Options);
    }
}
