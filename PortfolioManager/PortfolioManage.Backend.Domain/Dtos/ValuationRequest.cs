namespace PortfolioManage.Backend.Domain.Dtos;

public sealed record ValuationRequest(long CashValueRial, long StockValueRial, string? Note);