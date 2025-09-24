using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PandesalExpress.Infrastructure.Abstractions;
using PandesalExpress.Transfers.Dtos;
using PandesalExpress.Transfers.Exceptions;
using PandesalExpress.Transfers.Features.CreateTransferRequest;
using PandesalExpress.Transfers.Features.GetTransferRequest;
using PandesalExpress.Transfers.Features.GetTransferRequestsForStore;
using PandesalExpress.Transfers.Features.UpdateTransferRequestStatus;
using Shared.Dtos;

namespace PandesalExpress.Transfers.Controllers;

[Authorize]
[Route("/api")]
[ApiController]
public class TransfersController : ControllerBase
{
    [HttpPost("stores/{id}/request-transfer")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(TransferRequestDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TransferRequestDto>> RequestTransfer(
        [FromBody] CreateTransferRequestDto request,
        [FromServices] IMediator mediator,
        string id
    )
    {
        try
        {
            var command = new CreateTransferRequestCommand(
                request,
                Ulid.Parse(id)
            );

            TransferRequestDto result = await mediator.Send(command, HttpContext.RequestAborted);
            return CreatedAtAction(nameof(RequestTransfer), result);
        }
        catch (Exception) { return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong when requesting transfer. Please try again."); }
    }

    [HttpPut("[controller]/requests/{requestId}/update-status")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TransferRequestDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TransferRequestDto>> UpdateTransferRequestStatus(
        string requestId,
        [FromBody] UpdateTransferStatusDto request,
        [FromServices] IMediator mediator
    )
    {
        try
        {
            var command = new UpdateTransferRequestStatusCommand(
                Ulid.Parse(requestId),
                request,
                Ulid.Parse(User.FindFirst("sub")!.Value),
                User
            );

            TransferRequestDto result = await mediator.Send(command, HttpContext.RequestAborted);
            return Ok(result);
        }
        catch (Exception)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                "Something went wrong when updating transfer request status. Please try again."
            );
        }
    }

    [HttpGet("[controller]/requests/{requestId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TransferRequestDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TransferRequestDto>> GetTransferRequest(
        string requestId,
        [FromServices] IMediator mediator
    )
    {
        try
        {
            var query = new GetTransferRequestQuery(Ulid.Parse(requestId));
            TransferRequestDto result = await mediator.Send(query, HttpContext.RequestAborted);
            return Ok(result);
        }
        catch (Exception ex) when (ex is TransferRequestNotFoundException) { return NotFound($"Transfer request with ID {requestId} not found."); }
        catch (Exception)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                "Something went wrong when retrieving the transfer request. Please try again."
            );
        }
    }

    [HttpGet("stores/{id}/requests")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<TransferRequestDto>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<TransferRequestDto>>> GetTransferRequestsForStore(
        string id,
        [FromServices] IMediator mediator
    )
    {
        try
        {
            var query = new GetTransferRequestsForStoreQuery(Ulid.Parse(id));
            List<TransferRequestDto> results = await mediator.Send(query, HttpContext.RequestAborted);
            return Ok(results);
        }
        catch (Exception)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                "Something went wrong when retrieving transfer requests. Please try again."
            );
        }
    }
}
