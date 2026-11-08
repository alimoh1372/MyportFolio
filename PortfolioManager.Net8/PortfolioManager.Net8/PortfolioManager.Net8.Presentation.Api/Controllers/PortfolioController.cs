using Microsoft.AspNetCore.Mvc;
using PM.Net8.Application.Implementations;
using PM.Net8.Domain.Dtos;

namespace PM.Net8.Presentation.Api.Controllers;

[ApiController]
[Route("api")]
public sealed class PortfolioController(PortfolioService portfolioService) : ControllerBase
{
    [HttpGet("portfolio")]
    public async Task<IActionResult> GetPortfolio(
        CancellationToken cancellationToken)
    {
        var result = await portfolioService.GetSummaryAsync(cancellationToken);

        return Ok(result);
    }

    [HttpGet("operations")]
    public async Task<IActionResult> GetOperations(
        [FromQuery] int? take,
        CancellationToken cancellationToken)
    {
        var result = await portfolioService.GetOperationsAsync(
            take ?? 100,
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("partners")]
    public async Task<IActionResult> CreatePartner(
        [FromBody] CreatePartnerRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var id = await portfolioService.CreatePartnerAsync(
                request.Code,
                request.FirstName,
                request.LastName,
                cancellationToken);

            return Ok(new { id });
        }
        catch (Exception ex) when (
            ex is ArgumentException or InvalidOperationException)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("deposits")]
    public async Task<IActionResult> Deposit(
        [FromBody] CapitalRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await portfolioService.DepositAsync(
                request.PartnerCode,
                request.CurrentCashValueRial,
                request.CurrentStockValueRial,
                request.AmountRial,
                request.Note,
                cancellationToken);

            return Ok();
        }
        catch (Exception ex) when (
            ex is ArgumentException or InvalidOperationException)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("withdrawals")]
    public async Task<IActionResult> Withdraw(
        [FromBody] CapitalRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await portfolioService.WithdrawAsync(
                request.PartnerCode,
                request.CurrentCashValueRial,
                request.CurrentStockValueRial,
                request.AmountRial,
                request.Note,
                cancellationToken);

            return Ok();
        }
        catch (Exception ex) when (
            ex is ArgumentException or InvalidOperationException)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("portfolio/valuation")]
    public async Task<IActionResult> UpdateValuation(
        [FromBody] ValuationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await portfolioService.UpdateValuationAsync(
                request.CashValueRial,
                request.StockValueRial,
                request.Note,
                cancellationToken);

            return Ok();
        }
        catch (Exception ex) when (
            ex is ArgumentException or InvalidOperationException)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}