using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces;

namespace SubastaYa.Application.Features.Auctions.Queries;

public record GetAuctionBidsQuery(int AuctionId) : IQuery<IEnumerable<BidHistoryDto>>;