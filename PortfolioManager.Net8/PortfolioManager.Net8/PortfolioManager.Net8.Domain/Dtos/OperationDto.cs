namespace PM.Net8.Domain.Dtos;

public sealed record OperationDto(Guid Id, string Type, int? PartnerCode, string? PartnerName, long AmountRial, long BeforeRial, long AfterRial, long CashBeforeRial, long CashAfterRial, DateTime CreatedAtUtc, string? Note);