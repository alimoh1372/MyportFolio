using Microsoft.EntityFrameworkCore;
using PM.Net8.Application.Abstractions;
using PM.Net8.Domain.Entities;
using PM.Net8.Infrastructure.Contexts;

namespace PM.Net8.Infrastructure.Implementations;

public sealed class EfPortfolioRepository(PortfolioDbContext db) : IPortfolioRepository
{
    public async Task<Portfolio> GetPortfolioAsync(CancellationToken ct)
    {
        var p = await db.Portfolios.SingleOrDefaultAsync(ct); 
        if (p != null) return p;
        p = new Portfolio(); 
        db.Portfolios.Add(p); 
        await db.SaveChangesAsync(ct); 
        return p;
    }
    public Task<List<Partner>> GetPartnersAsync(CancellationToken ct) => db.Partners.AsNoTracking().OrderBy(x => x.Code).ToListAsync(ct);
    public Task<Partner?> GetPartnerAsync(int code, CancellationToken ct) => db.Partners.SingleOrDefaultAsync(x => x.Code == code, ct);
    public Task<List<PartnerShare>> GetSharesAsync(CancellationToken ct) => db.PartnerShares.ToListAsync(ct);
    public Task<List<PortfolioOperation>> GetOperationsAsync(int take, CancellationToken ct) => db.PortfolioOperations.AsNoTracking().OrderByDescending(x => x.CreatedAtUtc).Take(take).ToListAsync(ct);
    public void AddPartner(Partner x) => db.Partners.Add(x); public void AddShare(PartnerShare x) => db.PartnerShares.Add(x); public void AddOperation(PortfolioOperation x) => db.PortfolioOperations.Add(x); public void AddSnapshots(IEnumerable<ShareSnapshot> x) => db.ShareSnapshots.AddRange(x);
    public Task SaveAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
    public async Task InTransactionAsync(Func<Task> action, CancellationToken ct) { await using var tx = await db.Database.BeginTransactionAsync(ct); try { await action(); await tx.CommitAsync(ct); } catch { await tx.RollbackAsync(ct); throw; } }
}