namespace SubastaYa.Application.Services;

public interface IBidValidationService
{
    Task ValidateBidAsync(
        int auctionId,
        int buyerId,
        decimal amount,
        CancellationToken cancellationToken = default);
}
