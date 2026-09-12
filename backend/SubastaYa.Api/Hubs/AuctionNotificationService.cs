using Microsoft.AspNetCore.SignalR;
using SubastaYa.Application.Interfaces.Services;

namespace SubastaYa.Api.Hubs;

public class AuctionNotificationService : IAuctionNotificationService
{
    private readonly IHubContext<AuctionHub> _hubContext;

    public AuctionNotificationService(IHubContext<AuctionHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyNewBidAsync(int auctionId, decimal amount, int buyerId, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.Group(auctionId.ToString()).SendAsync("ReceiveNewBid", new
        {
            AuctionId = auctionId,
            Amount = amount,
            BuyerId = buyerId,
            Timestamp = DateTime.UtcNow
        }, cancellationToken);
    }

    public async Task NotifyTimeExtendedAsync(int auctionId, DateTime newEndDate, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.Group(auctionId.ToString()).SendAsync("AuctionTimeExtended", new
        {
            AuctionId = auctionId,
            NewEndDate = newEndDate
        }, cancellationToken);
    }

    public async Task NotifyAuctionClosedAsync(int auctionId, string status, decimal finalPrice, int? winnerId, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.Group(auctionId.ToString()).SendAsync("AuctionClosed", new
        {
            AuctionId = auctionId,
            Status = status,
            FinalPrice = finalPrice,
            WinnerId = winnerId
        }, cancellationToken);
    }
}