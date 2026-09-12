namespace SubastaYa.Application.Interfaces.Services;

public interface IAuctionNotificationService
{
    Task NotifyNewBidAsync(int auctionId, decimal amount, int buyerId, CancellationToken cancellationToken = default);
    Task NotifyTimeExtendedAsync(int auctionId, DateTime newEndDate, CancellationToken cancellationToken = default);
    Task NotifyAuctionClosedAsync(int auctionId, string status, decimal finalPrice, int? winnerId, CancellationToken cancellationToken = default);
}