using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces;

namespace SubastaYa.Application.Features.Wallets.Queries;

public class GetWalletTransactionsQuery : IQuery<IEnumerable<LedgerTransactionDto>>
{
    public int UserId { get; }

    public GetWalletTransactionsQuery(int userId)
    {
        UserId = userId;
    }
}