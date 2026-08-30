namespace SubastaYa.Application.DTOs;

public class LedgerTransactionDto
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public int? AuctionId { get; set; }
}