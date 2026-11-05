namespace PortfolioManage.Backend.Domain.Entities;

public sealed class Portfolio
{
    public Portfolio() { Id = Guid.NewGuid(); UpdatedAtUtc = DateTime.UtcNow; }
    public Guid Id { get; private set; }
    public long CashValueRial { get; private set; }
    public long StockValueRial { get; private set; }
    public long TotalValueRial => checked(CashValueRial + StockValueRial);
    public DateTime UpdatedAtUtc { get; private set; }
    public long Version { get; private set; }
    public void SetValues(long cashValueRial, long stockValueRial)
    {
        if (cashValueRial < 0 || stockValueRial < 0) throw new ArgumentOutOfRangeException(nameof(cashValueRial));

        _ = checked(cashValueRial + stockValueRial);

        CashValueRial = cashValueRial;
        
        StockValueRial = stockValueRial;

        UpdatedAtUtc = DateTime.UtcNow; 
        
        Version++;
    }
}