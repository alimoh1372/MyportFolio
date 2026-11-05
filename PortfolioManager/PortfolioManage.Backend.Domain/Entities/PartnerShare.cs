namespace PortfolioManage.Backend.Domain.Entities;

public sealed class PartnerShare
{
    private PartnerShare() { }

    public PartnerShare(Guid partnerId, long valueRial)
    {
        Id = Guid.NewGuid(); 

        PartnerId = partnerId; 
        
        SetValue(valueRial);
    }

    public Guid Id { get; private set; }
    public Guid PartnerId { get; private set; }
    public long ValueRial { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public void SetValue(long value) { if (value < 0) throw new ArgumentOutOfRangeException(nameof(value)); ValueRial = value; UpdatedAtUtc = DateTime.UtcNow; }
}