
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using PortfolioManage.Backend.Infrastructure.Contexts;

namespace PortfolioManage.Backend.Infrastructure.SqlServerMigrations;

public sealed class DesignTimeFactory : IDesignTimeDbContextFactory<PortfolioDbContext>
{
    public PortfolioDbContext CreateDbContext(string[] args)
    {
        var cs = Environment.GetEnvironmentVariable("ConnectionStrings__SqlServer") ?? "Server=localhost;Database=PortfolioManager;Trusted_Connection=True;TrustServerCertificate=True;";
        var options = new DbContextOptionsBuilder<PortfolioDbContext>().UseSqlServer(cs, x => x.MigrationsAssembly("PortfolioManager.Backend.Infrastructure")).Options;
        return new PortfolioDbContext(options);
    }
}