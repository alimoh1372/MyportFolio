using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PM.Net8.Application.Abstractions;
using PM.Net8.Application.Implementations;
using PM.Net8.Infrastructure.Contexts;
using PM.Net8.Infrastructure.Implementations;

namespace PM.Net8.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddPortfolioInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var provider = config["Database:Provider"] ?? "Sqlite";

        if (string.Equals(provider, "Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            var connectionString =
                config.GetConnectionString("Sqlite")
                ?? throw new InvalidOperationException(
                    "SQLite connection string is missing.");

            var connectionStringBuilder =
                new SqliteConnectionStringBuilder(connectionString);

            var dataSource = connectionStringBuilder.DataSource;

            if (!string.IsNullOrWhiteSpace(dataSource) &&
                dataSource != ":memory:" &&
                !dataSource.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
            {
                var fullPath = Path.GetFullPath(dataSource, AppContext.BaseDirectory);

                var directory = Path.GetDirectoryName(fullPath);

                if (!string.IsNullOrWhiteSpace(directory))
                    Directory.CreateDirectory(directory);

                connectionStringBuilder.DataSource = fullPath;
            }

            

            services.AddDbContext<PortfolioDbContext>(options =>
                options.UseSqlite(connectionString,
                    x => x.MigrationsAssembly("PortfolioManage.Backend.Infrastructure.SqliteMigrations")
                ));
        }
        else if (string.Equals(provider, "SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            var connectionString =
                config.GetConnectionString("SqlServer")
                ?? throw new InvalidOperationException(
                    "SQL Server connection string is missing.");

            services.AddDbContext<PortfolioDbContext>(options =>
                options.UseSqlServer(connectionString, x =>
                    x.MigrationsAssembly("PM.Net8.Infrastructure.SqlServerMigrations")));
        }
        else
        {
            throw new InvalidOperationException(
                $"Database provider '{provider}' is not supported.");
        }

        services.AddScoped<IPortfolioRepository, EfPortfolioRepository>(); services.AddScoped<PortfolioService>();
        
        return services;

        //var name = provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase) ?
        //    "SqlServer" : provider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase) ?
        //    "Sqlite"    : throw new InvalidOperationException($"Provider '{provider}' is not supported.");

        //var cs = config.GetConnectionString(name) ?? throw new InvalidOperationException($"ConnectionStrings:{name} is missing.");

        //services.AddDbContext<PortfolioDbContext>(o => {
        //    if (name == "SqlServer") o.UseSqlServer(cs, x => 
        //        x.MigrationsAssembly("PortfolioManage.Backend.Infrastructure.SqlServerMigrations")); 
        //    else o.UseSqlite(cs,
        //        x => x.MigrationsAssembly("PortfolioManage.Backend.Infrastructure.SqliteMigrations")
        //        );
        //});
        
       
    }
}