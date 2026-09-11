using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubastaYa.Application.DTOs;
using SubastaYa.Application.Features.Auctions.Commands;
using SubastaYa.Application.Features.Auctions.Commands.Handlers;
using SubastaYa.Application.Features.Auctions.Queries;
using SubastaYa.Application.Features.Auctions.Queries.Handlers;

namespace SubastaYa.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuctionsController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AuctionResponseDto>>> GetAll(
        [FromQuery] string? status,
        [FromQuery] int? categoryId,
        [FromServices] GetAuctionsHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new GetAuctionsQuery(status, categoryId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AuctionResponseDto>> GetById(
        int id,
        [FromServices] GetAuctionByIdHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new GetAuctionByIdQuery(id), cancellationToken);
        if (result == null)
            return NotFound(new { mensaje = $"Subasta con ID {id} no encontrada." });

        return Ok(result);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<AuctionResponseDto>> Create(
        [FromBody] CreateAuctionCommand command,
        [FromServices] CreateAuctionHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize]
    [HttpPost("bids")]
    public async Task<ActionResult<BidResponseDto>> PlaceBid(
        [FromBody] PlaceBidCommand command,
        [FromServices] PlaceBidHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(command, cancellationToken);
        return Ok(result);
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst("sub")?.Value;

        return int.Parse(claim!);
    }
}