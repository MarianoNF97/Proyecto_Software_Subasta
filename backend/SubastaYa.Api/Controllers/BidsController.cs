using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SubastaYa.Application.DTOs;
using SubastaYa.Application.Features.Auctions.Commands;
using SubastaYa.Application.Features.Auctions.Commands.Handlers;
using SubastaYa.Api.Hubs;

namespace SubastaYa.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[Route("api/auctions/bids")]
public class BidsController : ControllerBase
{
    private readonly IHubContext<AuctionHub> _hubContext;

    public BidsController(IHubContext<AuctionHub> hubContext)
    {
        _hubContext = hubContext;
    }

    [HttpPost]
    public async Task<ActionResult<BidResponseDto>> PlaceBid(
        [FromBody] PlaceBidCommand command,
        [FromServices] PlaceBidHandler handler,
        CancellationToken cancellationToken)
    {
        command.BuyerId = GetCurrentUserId();

        var result = await handler.HandleAsync(command, cancellationToken);

        // Notifica en tiempo real a los clientes conectados para refrescar sus saldos
        await _hubContext.Clients.All.SendAsync("WalletUpdated", cancellationToken);

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