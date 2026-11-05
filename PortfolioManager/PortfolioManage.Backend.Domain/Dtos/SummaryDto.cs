namespace PortfolioManage.Backend.Domain.Dtos;

public sealed record SummaryDto(long CashValueRial, long StockValueRial, long TotalValueRial, DateTime UpdatedAtUtc, IReadOnlyList<PartnerDto> Partners, decimal TotalOwnershipRatio);