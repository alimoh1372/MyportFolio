namespace PortfolioManage.Backend.Domain.Dtos;

public sealed record PartnerDto(Guid Id, int Code, string FullName, long ShareValueRial, decimal OwnershipRatio, bool IsActive);