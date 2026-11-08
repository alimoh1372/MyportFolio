using Microsoft.EntityFrameworkCore;
using PM.Net8.Domain.Entities;

namespace PM.Net8.Infrastructure.Contexts;

public sealed class PortfolioDbContext(DbContextOptions<PortfolioDbContext> options) : DbContext(options)
{
    public DbSet<Partner> Partners => Set<Partner>();
    public DbSet<Portfolio> Portfolios => Set<Portfolio>();
    public DbSet<PartnerShare> PartnerShares => Set<PartnerShare>();
    public DbSet<PortfolioOperation> PortfolioOperations => Set<PortfolioOperation>();
    public DbSet<ShareSnapshot> ShareSnapshots => Set<ShareSnapshot>();


    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Partner>(e =>
        {
            e.HasKey(x => x.Id);

            e.HasIndex(x => x.Code).IsUnique();

            e.Property(x => x.FirstName).HasMaxLength(100).IsRequired();

            e.Property(x => x.LastName).HasMaxLength(100).IsRequired();

            e.Ignore(x => x.FullName);

        });

        b.Entity<Portfolio>(e =>
        {
            e.HasKey(x => x.Id);

            e.Ignore(x => x.TotalValueRial);

            e.Property(x => x.Version).IsConcurrencyToken();

        });

        b.Entity<PartnerShare>(e =>
        {
            e.HasKey(x => x.Id);

            e.HasIndex(x => x.PartnerId).IsUnique();

        });

        b.Entity<PortfolioOperation>(e =>
        {
            e.HasKey(x => x.Id);

            e.HasIndex(x => x.CreatedAtUtc);

            e.Property(x => x.Note).HasMaxLength(1000);

        });

        b.Entity<ShareSnapshot>(e =>
        {
            e.HasKey(x => x.Id);

            e.HasIndex(x => new { x.OperationId, x.PartnerId }).IsUnique();

            e.Property(x => x.RatioBefore).HasPrecision(28, 12);

            e.Property(x => x.RatioAfter).HasPrecision(28, 12);

        });

    }
}