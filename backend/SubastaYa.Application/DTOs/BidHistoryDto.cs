namespace SubastaYa.Application.DTOs;

public record BidHistoryDto(
    int Id,
    int AuctionId,
    decimal Amount,
    DateTime CreatedAt,
    string BidderAlias
);