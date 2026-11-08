using PM.Net8.Domain.Enums;

namespace PM.Net8.Domain.Entities;

public sealed class PortfolioOperation
{
	private PortfolioOperation() { }

	public PortfolioOperation(
		OperationType type,
		Guid? partnerId,
		long amount,
		long before,
		long after,
		long cashBefore,
		long cashAfter,
		string? note
	)
	{
		Id = Guid.NewGuid();
		Type = type;
		PartnerId = partnerId;
		AmountRial = amount;
		PortfolioBeforeRial = before;
		PortfolioAfterRial = after;
		CashBeforeRial = cashBefore;
		CashAfterRial = cashAfter;
		Note = note;
		CreatedAtUtc = DateTime.UtcNow;
	}
	public Guid Id { get; private set; }
	public OperationType Type { get; private set; }
	public Guid? PartnerId { get; private set; }
	public long AmountRial { get; private set; }
	public long PortfolioBeforeRial { get; private set; }
	public long PortfolioAfterRial { get; private set; }
	public long CashBeforeRial { get; private set; }
	public long CashAfterRial { get; private set; }
	public string? Note { get; private set; }
	public DateTime CreatedAtUtc { get; private set; }
}