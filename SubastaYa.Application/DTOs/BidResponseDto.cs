namespace SubastaYa.Application.DTOs;

public class BidResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public decimal NewAmount { get; set; }
    public bool TimeExtended { get; set; }
    public DateTime NewEndDate { get; set; }
}