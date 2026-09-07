using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces;

namespace SubastaYa.Application.Features.Wallets.Queries;

public class GetWalletBalanceQuery : IQuery<WalletBalanceDto?>
{
    public int UserId { get; }

    public GetWalletBalanceQuery(int userId)
    {
        UserId = userId;
    }
}