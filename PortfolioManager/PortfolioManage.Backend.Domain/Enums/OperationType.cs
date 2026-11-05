namespace PortfolioManage.Backend.Domain.Enums;

public enum OperationType
{
    /// <summary>
    /// واریز
    /// </summary>
    Deposit = 1,

    /// <summary>
    /// برداشت
    /// </summary>
    Withdrawal = 2, 

    /// <summary>
    /// به روزرسانی مقادیر
    /// </summary>
    ValuationUpdate = 3
}