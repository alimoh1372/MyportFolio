namespace PM.Net8.Domain.Dtos;

// All amounts are Rials. Cash and stock values describe the portfolio immediately before the operation.
public sealed record CapitalRequest(int PartnerCode, long CurrentCashValueRial, long CurrentStockValueRial, long AmountRial, string? Note);