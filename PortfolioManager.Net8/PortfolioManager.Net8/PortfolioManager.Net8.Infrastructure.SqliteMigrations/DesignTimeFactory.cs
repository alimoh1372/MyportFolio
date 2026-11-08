using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using PM.Net8.Infrastructure.Contexts;

namespace PM.Net8.Infrastructure.SqliteMigrations;

public sealed class DesignTimeFactory : IDesignTimeDbContextFactory<PortfolioDbContext>
{
    public PortfolioDbContext CreateDbContext(string[] args)
    {
	    var baseDirectory = AppContext.BaseDirectory;

	    var configuration = new ConfigurationBuilder()
		    .SetBasePath(baseDirectory)
		    .AddJsonFile("appsettings.json", optional: false)
		    .Build();

		var cs = Environment.GetEnvironmentVariable("ConnectionStrings__Sqlite") ?? "Data Source=portfolio-manager.db";

        var options = new DbContextOptionsBuilder<PortfolioDbContext>().UseSqlite(
            cs, x => x.MigrationsAssembly("PortfolioManage.Backend.Infrastructure.SqliteMigrations")).Options;

        return new PortfolioDbContext(options);
    }
}