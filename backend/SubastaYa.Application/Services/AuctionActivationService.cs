using SubastaYa.Application.Interfaces.Repositories;
using SubastaYa.Application.Interfaces.Services;

namespace SubastaYa.Application.Services;

public class AuctionActivationService : IAuctionActivationService
{
    private readonly IAuctionRepository _auctionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AuctionActivationService(
        IAuctionRepository auctionRepository,
        IUnitOfWork unitOfWork)
    {
        _auctionRepository = auctionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> ActivateScheduledAuctionsAsync(CancellationToken cancellationToken = default)
    {
        var scheduledAuctions = await _auctionRepository.GetScheduledAuctionsToActivateAsync(cancellationToken);
        var auctionsList = scheduledAuctions.ToList();

        if (!auctionsList.Any())
            return 0;

        foreach (var auction in auctionsList)
        {
            auction.estado = "ACTIVA";
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return auctionsList.Count;
    }
}