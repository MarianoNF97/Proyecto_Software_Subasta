namespace SubastaYa.Application.DTOs;

public class WalletBalanceDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public decimal TotalBalance { get; set; }
    public decimal LockedBalance { get; set; }
    public decimal AvailableBalance { get; set; }
}