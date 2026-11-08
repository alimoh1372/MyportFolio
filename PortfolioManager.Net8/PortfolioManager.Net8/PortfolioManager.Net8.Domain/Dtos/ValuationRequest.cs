namespace PM.Net8.Domain.Dtos;

public sealed record ValuationRequest(long CashValueRial, long StockValueRial, string? Note);