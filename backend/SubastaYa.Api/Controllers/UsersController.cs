using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubastaYa.Application.DTOs;
using SubastaYa.Application.Features.Users.Queries;
using SubastaYa.Application.Features.Users.Queries.Handlers;

namespace SubastaYa.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly GetUserPurchasesHandler _purchasesHandler;
    private readonly GetUserAuctionsHandler _auctionsHandler;

    public UsersController(
        GetUserPurchasesHandler purchasesHandler,
        GetUserAuctionsHandler auctionsHandler)
    {
        _purchasesHandler = purchasesHandler;
        _auctionsHandler = auctionsHandler;
    }

    [HttpGet("my-purchases")]
    public async Task<ActionResult<IEnumerable<AuctionResponseDto>>> GetMyPurchases(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _purchasesHandler.HandleAsync(new GetUserPurchasesQuery(userId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("my-auctions")]
    public async Task<ActionResult<IEnumerable<AuctionResponseDto>>> GetMyAuctions(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _auctionsHandler.HandleAsync(new GetUserAuctionsQuery(userId), cancellationToken);
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