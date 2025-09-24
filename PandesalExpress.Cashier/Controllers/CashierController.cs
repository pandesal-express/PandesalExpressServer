using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PandesalExpress.Cashier.Exceptions;
using PandesalExpress.Cashier.Features.LogSales;
using PandesalExpress.Infrastructure.Abstractions;
using PandesalExpress.Infrastructure.Context;
using PandesalExpress.Infrastructure.Models;
using Shared.Dtos;

namespace PandesalExpress.Cashier.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class CashierController(AppDbContext context) : ControllerBase
{
	[HttpPost("{id}/verify-stocks")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<object>> VerifyStocks(string id)
	{
		if (!ModelState.IsValid) return BadRequest(ModelState);

		if (!Ulid.TryParse(id, out Ulid storeId)) return BadRequest("Invalid store ID format.");

		Store? store = await context.Stores.FindAsync(storeId);
		if (store == null) return NotFound($"Store with ID {storeId} not found.");

		store.StocksDateVerified = DateTime.UtcNow;
		context.Stores.Update(store);
		await context.SaveChangesAsync();

		return Ok(new { Message = "Stocks verified successfully." });
	}

	[HttpPost("{id}/log-sales")]
	[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LogSalesResponseDto))]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status409Conflict)] // For stock or concurrency issues
	public async Task<ActionResult<LogSalesResponseDto>> LogSalesTransaction(
		[FromBody] LogSalesRequestDto requestDto,
		string id,
		[FromServices] IMediator mediator
	)
	{
		if (!ModelState.IsValid) return BadRequest(ModelState);

		var storeUlid = Ulid.Parse(id);

		try
		{
			var query = new LogSalesQuery(storeUlid, User, requestDto);
			LogSalesResponseDto response = await mediator.Send(query, HttpContext.RequestAborted);

			return Ok(response);
		}
		catch (NotFoundException e) { return NotFound(new { message = e.Message }); }
		catch (ConflictException e) { return Conflict(new { message = e.Message }); }
		catch (Exception e) { return StatusCode(StatusCodes.Status500InternalServerError, new { message = e.Message }); }
	}
}
