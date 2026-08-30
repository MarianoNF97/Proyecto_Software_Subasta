using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces;
using SubastaYa.Application.Interfaces.Repositories;

namespace SubastaYa.Application.Features.Wallets.Queries.Handlers;

public class GetWalletTransactionsHandler : IQueryHandler<GetWalletTransactionsQuery, IEnumerable<LedgerTransactionDto>>
{
    private readonly IWalletRepository _walletRepository;

    public GetWalletTransactionsHandler(IWalletRepository walletRepository)
    {
        _walletRepository = walletRepository;
    }

    public async Task<IEnumerable<LedgerTransactionDto>> HandleAsync(GetWalletTransactionsQuery query, CancellationToken cancellationToken = default)
    {
        var wallet = await _walletRepository.GetByUserIdAsync(query.UserId, cancellationToken);
        if (wallet == null) return Enumerable.Empty<LedgerTransactionDto>();

        var transactions = await _walletRepository.GetTransactionsByWalletIdAsync(wallet.id, cancellationToken);

        return transactions.Select(t => new LedgerTransactionDto
        {
            Id = t.id,
            Type = t.tipo,
            Amount = t.monto,
            Date = t.fecha,
            AuctionId = t.subasta_id
        });
    }
}