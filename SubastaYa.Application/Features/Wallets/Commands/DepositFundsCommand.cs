using SubastaYa.Application.Interfaces;

namespace SubastaYa.Application.Features.Wallets.Commands;

public class DepositFundsCommand : ICommand<bool>
{
    public int UserId { get; set; }
    public decimal Amount { get; set; }
}