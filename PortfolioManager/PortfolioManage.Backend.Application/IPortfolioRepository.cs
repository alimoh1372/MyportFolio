using PortfolioManage.Backend.Domain.Entities;

namespace PortfolioManage.Backend.Application;

public interface IPortfolioRepository
{
    Task<Portfolio> GetPortfolioAsync(CancellationToken ct);
    Task<List<Partner>> GetPartnersAsync(CancellationToken ct);
    Task<Partner?> GetPartnerAsync(int code, CancellationToken ct);
    Task<List<PartnerShare>> GetSharesAsync(CancellationToken ct);
    Task<List<PortfolioOperation>> GetOperationsAsync(int take, CancellationToken ct);
    void AddPartner(Partner partner); void AddShare(PartnerShare share);
    void AddOperation(PortfolioOperation operation); void AddSnapshots(IEnumerable<ShareSnapshot> snapshots);
    Task SaveAsync(CancellationToken ct);
    Task InTransactionAsync(Func<Task> action, CancellationToken ct);
}