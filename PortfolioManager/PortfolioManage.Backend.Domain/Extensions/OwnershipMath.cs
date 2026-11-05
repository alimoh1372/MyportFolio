namespace PortfolioManage.Backend.Domain.Extensions;

public static class OwnershipMath
{
    public static decimal ToPercent(decimal ownershipRatio) => ownershipRatio * 100m;
}
