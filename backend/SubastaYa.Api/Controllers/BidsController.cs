using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubastaYa.Application.DTOs;
using SubastaYa.Application.Features.Auctions.Commands;
using SubastaYa.Application.Features.Auctions.Commands.Handlers;

namespace SubastaYa.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]   
[Route("api/auctions/bids")]  
public class BidsController : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<BidResponseDto>> PlaceBid(
        [FromBody] PlaceBidCommand command,
        [FromServices] PlaceBidHandler handler,
        CancellationToken cancellationToken)
    {
        command.BuyerId = GetCurrentUserId();

        var result = await handler.HandleAsync(command, cancellationToken);
        return Ok(result);
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var userId))
        {
            throw new UnauthorizedAccessException("El token no contiene un identificador de usuario válido.");
        }

        return userId;
    }
}