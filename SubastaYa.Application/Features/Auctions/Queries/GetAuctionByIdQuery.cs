using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces;

namespace SubastaYa.Application.Features.Auctions.Queries;

public class GetAuctionByIdQuery : IQuery<AuctionResponseDto?>
{
    public int Id { get; }

    public GetAuctionByIdQuery(int id)
    {
        Id = id;
    }
}