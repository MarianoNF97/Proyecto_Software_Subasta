namespace SubastaYa.Application.Services;

public interface IBidPaymentService
{
    Task ProcessBidPaymentAsync(
        int auctionId,
        int newBidderId,
        decimal amount,
        CancellationToken cancellationToken = default);
}
