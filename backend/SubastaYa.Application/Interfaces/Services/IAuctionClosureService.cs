namespace SubastaYa.Application.Services;

public interface IAuctionClosureService
{
    Task<int> CloseExpiredAuctionsAsync(CancellationToken cancellationToken = default);
}
