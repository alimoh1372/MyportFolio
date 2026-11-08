using PM.Net8.Application.Abstractions;
using PM.Net8.Domain.Dtos;
using PM.Net8.Domain.Entities;
using PM.Net8.Domain.Enums;

namespace PM.Net8.Application.Implementations;

public sealed class PortfolioService(IPortfolioRepository db)
{
	private const decimal AllocationRate = 0.01m;

	public async Task<Guid> CreatePartnerAsync(int code, string firstName, string lastName, CancellationToken ct)
	{
		if (await db.GetPartnerAsync(code, ct) is not null)
			throw new InvalidOperationException("کد شریک تکراری است.");

		var p = new Partner(code, firstName, lastName);

		db.AddPartner(p);

		await db.SaveAsync(ct);

		return p.Id;
	}

	public Task DepositAsync(int partnerCode, long currentCash, long currentStock, long amount, string? note, CancellationToken ct) => db.InTransactionAsync(async () =>
	{
		Positive(amount);

		ValidatePortfolioValues(currentCash, currentStock);

		var currentTotal = checked(currentCash + currentStock);

		var partner = await db.GetPartnerAsync(partnerCode, ct) ?? throw new InvalidOperationException("شریک پیدا نشد.");

		if (!partner.IsActive) throw new InvalidOperationException("شریک غیرفعال است.");

		var portfolio = await db.GetPortfolioAsync(ct); var shares = await db.GetSharesAsync(ct);

		var before = Revalue(shares, currentTotal);

		var owners = before.Where(x => x.Value > 0).ToArray();

		// همان فرمول کاربر: increase * (stock / total portfolio) * 1%.
		var stockRatio = currentTotal == 0 ? 0m : (decimal)currentStock / currentTotal;

		var allocationPool = owners.Length == 0 ? 0L : (long)decimal.Floor(amount * stockRatio * AllocationRate);

		var afterTotal = checked(currentTotal + amount);

		var after = new Dictionary<Guid, long>(before);

		if (owners.Length == 0)
		{
			if (currentTotal != 0) throw new InvalidOperationException("برای پرتفو با ارزش مثبت، سهم شرکا ثبت نشده است.");
			after[partner.Id] = amount;
		}
		else
		{
			DistributeAllocation(owners, currentTotal, allocationPool, after);
			after[partner.Id] = checked(after.GetValueOrDefault(partner.Id) + amount - allocationPool);
		}

		EnsureTotal(after, afterTotal);
		var afterCash = checked(currentCash + amount);
		var op = new PortfolioOperation(OperationType.Deposit, partner.Id, amount, currentTotal, afterTotal, currentCash, afterCash, note);
		Persist(shares, before, after, currentTotal, afterTotal, op);
		portfolio.SetValues(afterCash, currentStock); db.AddOperation(op); await db.SaveAsync(ct);
	}, ct);

	public Task WithdrawAsync(int partnerCode, long currentCash, long currentStock, long amount, string? note, CancellationToken ct) => db.InTransactionAsync(async () =>
	{
		Positive(amount); ValidatePortfolioValues(currentCash, currentStock);
		var currentTotal = checked(currentCash + currentStock);
		if (currentTotal <= 0) throw new InvalidOperationException("ارزش کل پرتفو باید بیشتر از صفر باشد.");
		if (currentCash < amount) throw new InvalidOperationException($"وجه نقد کافی نیست. وجه نقد فعلی: {currentCash:N0} ریال. ابتدا باید به اندازه کسری، سهام فروخته شود.");
		var partner = await db.GetPartnerAsync(partnerCode, ct) ?? throw new InvalidOperationException("شریک پیدا نشد.");
		if (!partner.IsActive) throw new InvalidOperationException("شریک غیرفعال است.");
		var portfolio = await db.GetPortfolioAsync(ct); var shares = await db.GetSharesAsync(ct);
		var before = Revalue(shares, currentTotal); var available = before.GetValueOrDefault(partner.Id);
		if (amount > available) throw new InvalidOperationException($"مبلغ برداشت از سهم شریک بیشتر است. سهم فعلی: {available:N0} ریال");
		var afterTotal = currentTotal - amount;
		var after = new Dictionary<Guid, long>(before) { [partner.Id] = available - amount };
		EnsureTotal(after, afterTotal);
		var afterCash = currentCash - amount;
		var op = new PortfolioOperation(OperationType.Withdrawal, partner.Id, amount, currentTotal, afterTotal, currentCash, afterCash, note);
		Persist(shares, before, after, currentTotal, afterTotal, op);
		portfolio.SetValues(afterCash, currentStock); db.AddOperation(op); await db.SaveAsync(ct);
	}, ct);

	public Task UpdateValuationAsync(long cashValue, long stockValue, string? note, CancellationToken ct) =>
		db.InTransactionAsync(async () =>
		{

			ValidatePortfolioValues(cashValue, stockValue);

			var newTotal = checked(cashValue + stockValue);

			var portfolio = await db.GetPortfolioAsync(ct);

			var shares = await db.GetSharesAsync(ct);

			var beforeTotal = portfolio.TotalValueRial;

			var before = shares.ToDictionary(x => x.PartnerId, x => x.ValueRial);

			Dictionary<Guid, long> after;

			if (shares.Count == 0 && newTotal > 0)
			{
				var owner = (await db.GetPartnersAsync(ct))
							.SingleOrDefault(x => x.IsActive)
							?? throw new InvalidOperationException("ابتدا باید یک شریک فعال ثبت کنید.");

				after = new Dictionary<Guid, long>
				{
					[owner.Id] = newTotal
				};
			}
			else
			{
				after = Revalue(shares, newTotal);
			}

			EnsureTotal(after, newTotal);

			var op = new PortfolioOperation(OperationType.ValuationUpdate, null, Math.Abs(newTotal - beforeTotal), beforeTotal, newTotal, portfolio.CashValueRial, cashValue, note);

			Persist(shares, before, after, beforeTotal, newTotal, op);

			portfolio.SetValues(cashValue, stockValue);

			db.AddOperation(op); await db.SaveAsync(ct);

		}, ct);

	public async Task<SummaryDto> GetSummaryAsync(CancellationToken ct)
	{
		var p = await db.GetPortfolioAsync(ct);
		var partners = await db.GetPartnersAsync(ct);
		var shares = (await db.GetSharesAsync(ct)).ToDictionary(x => x.PartnerId);
		var rows = partners.Select(x =>
		{
			var value = shares.GetValueOrDefault(x.Id)?.ValueRial ?? 0;
			var ratio = p.TotalValueRial == 0 ? 0m : (decimal)value / p.TotalValueRial;
			return new PartnerDto(x.Id, x.Code, x.FullName, value, ratio, x.IsActive);
		}).OrderBy(x => x.Code).ToArray();
		return new SummaryDto(p.CashValueRial, p.StockValueRial, p.TotalValueRial, p.UpdatedAtUtc, rows, rows.Sum(x => x.OwnershipRatio));
	}

	public async Task<IReadOnlyList<OperationDto>> GetOperationsAsync(int take, CancellationToken ct)
	{
		var ops = await db.GetOperationsAsync(Math.Clamp(take, 1, 500), ct); var partners = (await db.GetPartnersAsync(ct)).ToDictionary(x => x.Id);
		return ops.Select(o => { partners.TryGetValue(o.PartnerId ?? Guid.Empty, out var p); return new OperationDto(o.Id, o.Type.ToString(), p?.Code, p?.FullName, o.AmountRial, o.PortfolioBeforeRial, o.PortfolioAfterRial, o.CashBeforeRial, o.CashAfterRial, o.CreatedAtUtc, o.Note); }).ToArray();
	}

	private static void DistributeAllocation(KeyValuePair<Guid, long>[] owners, long total, long pool, Dictionary<Guid, long> after)
	{
		long distributed = 0;
		for (var i = 0; i < owners.Length; i++)
		{
			var (partnerId, value) = owners[i];
			var allocation = i == owners.Length - 1 ? pool - distributed : (long)decimal.Floor((decimal)pool * value / total);
			distributed += allocation;
			after[partnerId] = checked(after.GetValueOrDefault(partnerId) + allocation);
		}
	}

	private static Dictionary<Guid, long> Revalue(List<PartnerShare> shares, long total)
	{
		var owners = shares
			.Where(x => x.ValueRial > 0)
			.ToArray();

		if (owners.Length == 0)
		{
			if (total != 0) throw new InvalidOperationException("برای پرتفو با ارزش مثبت، مالک ثبت نشده است.");

			return new();
		}
		var oldSum = owners
			.Sum(x => x.ValueRial);

		long distributed = 0;

		var result = new Dictionary<Guid, long>();

		for (var i = 0; i < owners.Length; i++)
		{
			var x = owners[i];

			var value = i == owners.Length - 1 ?
				total - distributed :
				(long)decimal.Floor((decimal)total * x.ValueRial / oldSum);

			distributed += value;

			result[x.PartnerId] = value;
		}
		return result;
	}

	private void Persist(List<PartnerShare> shares, Dictionary<Guid, long> before, Dictionary<Guid, long> after, long beforeTotal, long afterTotal, PortfolioOperation op)
	{
		var existing = shares.ToDictionary(x => x.PartnerId);
		foreach (var id in before.Keys.Union(after.Keys).Distinct())
		{
			var old = before.GetValueOrDefault(id); var value = after.GetValueOrDefault(id);
			if (existing.TryGetValue(id, out var share)) share.SetValue(value); else db.AddShare(new PartnerShare(id, value));
			var ratioBefore = beforeTotal == 0 ? 0m : (decimal)old / beforeTotal;
			var ratioAfter = afterTotal == 0 ? 0m : (decimal)value / afterTotal;
			db.AddSnapshots(new[] { new ShareSnapshot(op.Id, id, old, value, ratioBefore, ratioAfter) });
		}
	}

	private static void EnsureTotal(Dictionary<Guid, long> values, long expected)
	{ if (values.Values.Sum() != expected) throw new InvalidOperationException("جمع سهم شرکا با ارزش کل پرتفو برابر نیست."); }
	private static void ValidatePortfolioValues(long cash, long stock)
	{ if (cash < 0 || stock < 0) throw new ArgumentOutOfRangeException(nameof(cash), "ارزش وجه نقد و سهام نمی‌تواند منفی باشد."); _ = checked(cash + stock); }
	private static void Positive(long x) { if (x <= 0) throw new ArgumentOutOfRangeException(nameof(x), "مبلغ باید مثبت باشد."); }
}
