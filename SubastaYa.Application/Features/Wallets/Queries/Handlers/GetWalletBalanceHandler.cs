using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces;
using SubastaYa.Application.Interfaces.Repositories;

namespace SubastaYa.Application.Features.Wallets.Queries.Handlers;

public class GetWalletBalanceHandler : IQueryHandler<GetWalletBalanceQuery, WalletBalanceDto?>
{
    private readonly IWalletRepository _walletRepository;

    public GetWalletBalanceHandler(IWalletRepository walletRepository)
    {
        _walletRepository = walletRepository;
    }

    public async Task<WalletBalanceDto?> HandleAsync(GetWalletBalanceQuery query, CancellationToken cancellationToken = default)
    {
        var wallet = await _walletRepository.GetByUserIdAsync(query.UserId, cancellationToken);
        if (wallet == null) return null;

        return new WalletBalanceDto
        {
            Id = wallet.id,
            UserId = wallet.usuario_id,
            TotalBalance = wallet.saldo_total,
            LockedBalance = wallet.saldo_retenido,
            AvailableBalance = wallet.saldo_disponible
        };
    }
}