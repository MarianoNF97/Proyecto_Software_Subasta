using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces;
using SubastaYa.Application.Interfaces.Repositories;

namespace SubastaYa.Application.Features.Auctions.Queries.Handlers;

public class GetAuctionBidsHandler : IQueryHandler<GetAuctionBidsQuery, IEnumerable<BidHistoryDto>>
{
    private readonly IAuctionRepository _auctionRepository;

    public GetAuctionBidsHandler(IAuctionRepository auctionRepository)
    {
        _auctionRepository = auctionRepository;
    }

    public async Task<IEnumerable<BidHistoryDto>> HandleAsync(GetAuctionBidsQuery query, CancellationToken cancellationToken = default)
    {
        var bids = await _auctionRepository.GetBidsByAuctionIdAsync(query.AuctionId, cancellationToken);

        return bids.Select(b => new BidHistoryDto(
            b.id,
            b.subasta_id,
            b.monto,
            b.fecha_puja,
            $"user_{b.comprador_id.ToString().PadLeft(4, '0')}"
        ));
    }
}