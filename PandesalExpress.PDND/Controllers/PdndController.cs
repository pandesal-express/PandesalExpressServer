using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PandesalExpress.Infrastructure.Abstractions;
using PandesalExpress.PDND.Dtos;
using PandesalExpress.PDND.Features.CreatePdndRequest;
using PandesalExpress.PDND.Features.GetPdndRequest;
using PandesalExpress.PDND.Features.GetPdndRequests;
using PandesalExpress.PDND.Features.UpdatePdndStatus;
using Shared.Dtos;

namespace PandesalExpress.PDND.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class PdndController : ControllerBase
{
    [HttpPost("Stores/{id}/request-pdnd")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(PdndRequestDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PdndRequestDto>> RequestPdnd(
        [FromBody] CreatePdndRequestDto request,
        [FromServices] IMediator mediator,
        string id
    )
    {
        try
        {
            var command = new CreatePdndRequestCommand(
                id,
                request.BranchCode,
                request.CashierId,
                request.DateNeeded,
                request.Items
            );
            PdndRequestDto result = await mediator.Send(command, HttpContext.RequestAborted);

            return CreatedAtAction(nameof(RequestPdnd), result);
        }
        catch (Exception) { return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong when requesting PDND. Please try again."); }
    }

    [HttpPut("requests/{requestId}/status")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PdndStatusUpdateResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PdndStatusUpdateResponseDto>> UpdatePdndStatus(
        string requestId,
        [FromBody] UpdatePdndStatusRequestDto request,
        [FromServices] IMediator mediator
    )
    {
        try
        {
            var command = new UpdatePdndStatusCommand(
                requestId,
                request.NewStatus,
                request.Notes,
                User
            );

            PdndStatusUpdateResponseDto result = await mediator.Send(command, HttpContext.RequestAborted);
            return Ok(result);
        }
        catch (Exception) { return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong when updating PDND status. Please try again."); }
    }

    [HttpGet("requests/{requestId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PdndRequestDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PdndRequestDto>> GetPdndRequest(
        string requestId,
        [FromServices] IMediator mediator
    )
    {
        try
        {
            var query = new GetPdndRequestQuery(requestId, User);
            PdndRequestDto result = await mediator.Send(query, HttpContext.RequestAborted);
            return Ok(result);
        }
        catch (Exception) { return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong when retrieving PDND request. Please try again."); }
    }

    [HttpGet("requests")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PdndRequestsResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PdndRequestsResponseDto>> GetPdndRequests(
        [FromServices] IMediator mediator,
        [FromQuery] GetPdndRequestsQuery query
    )
    {
        try
        {
            query.User = User;

            PdndRequestsResponseDto result = await mediator.Send(query, HttpContext.RequestAborted);
            return Ok(result);
        }
        catch (Exception) { return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong when retrieving PDND requests. Please try again."); }
    }
}
