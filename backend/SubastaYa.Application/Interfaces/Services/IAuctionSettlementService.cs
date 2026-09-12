using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Interfaces.Services;

public interface IAuctionSettlementService
{
    Task SettleAuctionPaymentAsync(Subasta subasta, Puja winningBid, DateTime fechaUtc, CancellationToken cancellationToken = default);
}