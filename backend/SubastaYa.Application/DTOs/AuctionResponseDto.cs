namespace SubastaYa.Application.DTOs;

public class AuctionResponseDto
{
    public int Id { get; set; }
    public int SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal StartingPrice { get; set; }
    public decimal MinIncrement { get; set; }
    public decimal CurrentPrice { get; set; }
    public int TotalBids { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? WinningBidderId { get; set; }
}