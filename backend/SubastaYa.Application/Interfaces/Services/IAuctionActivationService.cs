namespace SubastaYa.Application.Interfaces.Services;

public interface IAuctionActivationService
{
    Task<int> ActivateScheduledAuctionsAsync(CancellationToken cancellationToken = default);
}