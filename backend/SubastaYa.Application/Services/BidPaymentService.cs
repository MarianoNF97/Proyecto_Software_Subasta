using SubastaYa.Application.Interfaces.Repositories;
using SubastaYa.Domain.Constants;
using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Services;

/// <summary>
/// Implementación del servicio de pago de pujas.
/// Responsabilidad única: gestionar débitos y créditos de fondos para pujas.
/// </summary>
public class BidPaymentService : IBidPaymentService
{
    private readonly IAuctionRepository _auctionRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IBidWinnerService _bidWinnerService;

    public BidPaymentService(
        IAuctionRepository auctionRepository,
        IWalletRepository walletRepository,
        IBidWinnerService bidWinnerService)
    {
        _auctionRepository = auctionRepository;
        _walletRepository = walletRepository;
        _bidWinnerService = bidWinnerService;
    }

    public async Task ProcessBidPaymentAsync(
        int auctionId,
        int newBidderId,
        decimal amount,
        CancellationToken cancellationToken = default)
    {
        var subasta = await _auctionRepository.GetByIdWithBidsAsync(auctionId, cancellationToken);
        if (subasta == null) return;

        var ahoraUtc = DateTime.UtcNow;
        var highestBid = _bidWinnerService.GetWinningBid(subasta.Pujas);

        // Liberar saldo del postor anterior
        if (highestBid != null)
        {
            var previousBuyerWallet = await _walletRepository.GetByUserIdAsync(highestBid.comprador_id, cancellationToken);
            if (previousBuyerWallet != null)
            {
                previousBuyerWallet.saldo_retenido -= highestBid.monto;

                _walletRepository.AddTransaction(new TransaccionLedger
                {
                    billetera_id = previousBuyerWallet.id,
                    tipo = TransactionConstants.TIPO_LIBERACION,
                    monto = highestBid.monto,
                    fecha = ahoraUtc,
                    subasta_id = auctionId
                });
            }
        }

        // Retener saldo del nuevo postor
        var buyerWallet = await _walletRepository.GetByUserIdAsync(newBidderId, cancellationToken);
        if (buyerWallet != null)
        {
            buyerWallet.saldo_retenido += amount;

            _walletRepository.AddTransaction(new TransaccionLedger
            {
                billetera_id = buyerWallet.id,
                tipo = TransactionConstants.TIPO_RETENCION,
                monto = amount,
                fecha = ahoraUtc,
                subasta_id = auctionId
            });
        }
    }
}
