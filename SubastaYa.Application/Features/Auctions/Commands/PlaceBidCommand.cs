using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces;

namespace SubastaYa.Application.Features.Auctions.Commands;

public class PlaceBidCommand : ICommand<BidResponseDto>
{
    public int AuctionId { get; set; }
    public int BuyerId { get; set; }
    public decimal Amount { get; set; }
}