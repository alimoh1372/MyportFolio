
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using PortfolioManage.Backend.Infrastructure.Contexts;

namespace PortfolioManage.Backend.Infrastructure.SqliteMigrations;

public sealed class DesignTimeFactory : IDesignTimeDbContextFactory<PortfolioDbContext>
{
    public PortfolioDbContext CreateDbContext(string[] args)
    {
        var cs = Environment.GetEnvironmentVariable("ConnectionStrings__Sqlite") ?? "Data Source=portfolio-manager.db";
        var options = new DbContextOptionsBuilder<PortfolioDbContext>().UseSqlite(
            cs, x => x.MigrationsAssembly("PortfolioManage.Backend.Infrastructure.SqliteMigrations")).Options;
        return new PortfolioDbContext(options);
    }
}