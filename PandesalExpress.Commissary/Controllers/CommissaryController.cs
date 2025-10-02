using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PandesalExpress.Commissary.Features.AddStocksToStore;
using PandesalExpress.Infrastructure.Abstractions;
using Shared.Dtos;

namespace PandesalExpress.Commissary.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class CommissaryController : ControllerBase
{
    [HttpPost("stores/{id}/add-stocks")]
    [Authorize]
    [ProducesResponseType(typeof(AddStocksToStoreResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddStocksToStore(
        string id,
        [FromBody] DeliverStocksRequestDto request,
        [FromServices] ICommandHandler<AddStocksToStoreCommand, AddStocksToStoreResponseDto> handler)
    {
        try
        {
            var command = new AddStocksToStoreCommand(id, request, User);
            AddStocksToStoreResponseDto result = await handler.Handle(command, HttpContext.RequestAborted);

            return Ok(result);
        }
        catch (DBConcurrencyException)
        {
            return BadRequest(new { message = "Something went wrong when adding stocks to store. Please try again." });
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }
}
