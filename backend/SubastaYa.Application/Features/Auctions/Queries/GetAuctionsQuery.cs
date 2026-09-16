using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces;

namespace SubastaYa.Application.Features.Auctions.Queries;

public class GetAuctionsQuery : IQuery<IEnumerable<AuctionResponseDto>>
{
    public string? Status { get; }
    public int? CategoryId { get; }
    public decimal? MinPrice { get; }
    public decimal? MaxPrice { get; }

    public GetAuctionsQuery(
        string? status = null,
        int? categoryId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null)
    {
        Status = status;
        CategoryId = categoryId;
        MinPrice = minPrice;
        MaxPrice = maxPrice;
    }
}