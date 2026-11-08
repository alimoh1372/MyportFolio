using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using PM.Net8.Infrastructure.Contexts;

namespace PM.Net8.Infrastructure.SqlServerMigrations;

public sealed class DesignTimeFactory : IDesignTimeDbContextFactory<PortfolioDbContext>
{
	public PortfolioDbContext CreateDbContext(string[] args)
	{
		var baseDirectory = AppContext.BaseDirectory;

		var configurations = new ConfigurationBuilder()
			.SetBasePath(baseDirectory)
			.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
			.Build();


		var provider = configurations["Database:Provider"];

		if (provider is null)
			throw new ArgumentNullException("Database:Provider",
				"the appsetting.json file must have Database.Provider item.");

		if (string.Equals(provider, "SqlServer", StringComparison.OrdinalIgnoreCase) is false)
			throw new InvalidOperationException("Database.Provider: in appsetting must be sqlServer....");



		var connectionString =
			configurations.GetConnectionString("SqlServer")
			?? throw new InvalidOperationException(
				"SqlServer connection string is missing.");


		var options = new DbContextOptionsBuilder<PortfolioDbContext>().
			UseSqlServer(connectionString, x => 
				x.MigrationsAssembly("PM.Net8.Infrastructure.SqlServerMigrations")).Options;

		return new PortfolioDbContext(options);
	}
}