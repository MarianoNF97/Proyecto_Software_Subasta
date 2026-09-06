using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces;

namespace SubastaYa.Application.Features.Auctions.Queries;

public class GetAuctionsQuery : IQuery<IEnumerable<AuctionResponseDto>>
{
    public string? Status { get; }
    public int? CategoryId { get; }

    public GetAuctionsQuery(string? status = null, int? categoryId = null)
    {
        Status = status;
        CategoryId = categoryId;
    }
}