namespace PortfolioManage.Backend.Domain.Entities;

public sealed class Partner
{
    private Partner() { }
    public Partner(int code, string firstName, string lastName)
    {
        if (code <= 0) throw new ArgumentOutOfRangeException(nameof(code));

        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("نام و نام خانوادگی الزامی است.");

        Id = Guid.NewGuid(); Code = code; FirstName = firstName.Trim(); LastName = lastName.Trim(); IsActive = true;
    }
    public Guid Id { get; private set; }

    public int Code { get; private set; }

    public string FirstName { get; private set; } = "";

    public string LastName { get; private set; } = "";

    public bool IsActive { get; private set; }

    public string FullName => $"{FirstName} {LastName}";
}