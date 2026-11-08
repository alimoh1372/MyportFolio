namespace PM.Net8.Domain.Entities;

public sealed class ShareSnapshot
{
	private ShareSnapshot() { }

	public ShareSnapshot(
		Guid operationId,
		Guid partnerId,
		long before,
		long after,
		decimal ratioBefore,
		decimal ratioAfter
	)
	{
		Id = Guid.NewGuid();
		OperationId = operationId;
		PartnerId = partnerId;
		ValueBeforeRial = before;
		ValueAfterRial = after;
		RatioBefore = ratioBefore;
		RatioAfter = ratioAfter;
	}
	public Guid Id { get; private set; }
	public Guid OperationId { get; private set; }
	public Guid PartnerId { get; private set; }
	public long ValueBeforeRial { get; private set; }
	public long ValueAfterRial { get; private set; }
	// Ownership ratio in [0,1], not a percentage in [0,100].
	public decimal RatioBefore { get; private set; }
	public decimal RatioAfter { get; private set; }
}