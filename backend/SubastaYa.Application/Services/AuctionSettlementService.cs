using SubastaYa.Application.Interfaces.Repositories;
using SubastaYa.Application.Interfaces.Services;
using SubastaYa.Domain.Constants;
using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Services;

public class AuctionSettlementService : IAuctionSettlementService
{
    private readonly IWalletRepository _walletRepository;

    public AuctionSettlementService(IWalletRepository walletRepository)
    {
        _walletRepository = walletRepository;
    }

    public async Task SettleAuctionPaymentAsync(Subasta subasta, Puja winningBid, DateTime fechaUtc, CancellationToken cancellationToken = default)
    {
        // 1. Débito definitivo al comprador (se quita de retenido y de total)
        var buyerWallet = await _walletRepository.GetByUserIdAsync(winningBid.comprador_id, cancellationToken);
        if (buyerWallet != null)
        {
            buyerWallet.saldo_retenido = Math.Max(0, buyerWallet.saldo_retenido - winningBid.monto);
            buyerWallet.saldo_total -= winningBid.monto;
            buyerWallet.saldo_disponible = buyerWallet.saldo_total - buyerWallet.saldo_retenido;

            _walletRepository.AddTransaction(new TransaccionLedger
            {
                billetera_id = buyerWallet.id,
                tipo = TransactionConstants.TIPO_PAGO,
                monto = winningBid.monto,
                fecha = fechaUtc,
                subasta_id = subasta.id
            });
        }

        // 2. Acreditación de fondos al vendedor (impacta en total y en disponible)
        var sellerWallet = await _walletRepository.GetByUserIdAsync(subasta.vendedor_id, cancellationToken);
        if (sellerWallet != null)
        {
            sellerWallet.saldo_total += winningBid.monto;
            sellerWallet.saldo_disponible = sellerWallet.saldo_total - sellerWallet.saldo_retenido;

            _walletRepository.AddTransaction(new TransaccionLedger
            {
                billetera_id = sellerWallet.id,
                tipo = TransactionConstants.TIPO_COBRO,
                monto = winningBid.monto,
                fecha = fechaUtc,
                subasta_id = subasta.id
            });
        }
    }
}