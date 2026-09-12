using SubastaYa.Application.Exceptions;
using SubastaYa.Application.Interfaces.Repositories;
using SubastaYa.Domain.Constants;

namespace SubastaYa.Application.Services;

/// <summary>
/// Implementación del servicio de validación de pujas.
/// Responsabilidad única: validar reglas de negocio para pujas.
/// </summary>
public class BidValidationService : IBidValidationService
{
    private readonly IAuctionRepository _auctionRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IBidWinnerService _bidWinnerService;

    public BidValidationService(
        IAuctionRepository auctionRepository,
        IWalletRepository walletRepository,
        IBidWinnerService bidWinnerService)
    {
        _auctionRepository = auctionRepository;
        _walletRepository = walletRepository;
        _bidWinnerService = bidWinnerService;
    }

    public async Task ValidateBidAsync(
        int auctionId,
        int buyerId,
        decimal amount,
        CancellationToken cancellationToken = default)
    {
        var subasta = await _auctionRepository.GetByIdWithBidsAsync(auctionId, cancellationToken)
            ?? throw new NotFoundException($"No se encontró la subasta con ID {auctionId}.");

        if (subasta.estado != AuctionConstants.ESTADO_ACTIVA || DateTime.UtcNow > subasta.fecha_fin)
            throw new BusinessValidationException("La subasta no se encuentra activa para recibir ofertas.");

        if (subasta.vendedor_id == buyerId)
            throw new BusinessValidationException("El vendedor no puede ofertar en su propia subasta.");

        var highestBid = _bidWinnerService.GetWinningBid(subasta.Pujas);
        decimal minAllowedBid = highestBid != null
            ? highestBid.monto + subasta.incremento_minimo
            : subasta.precio_base;

        if (amount < minAllowedBid)
            throw new BusinessValidationException($"La oferta debe ser de al menos ${minAllowedBid}.");

        if (highestBid != null && highestBid.comprador_id == buyerId)
            throw new BusinessValidationException("Ya eres el postor líder de esta subasta.");

        // Validación de fondos disponibles reales (Total - Retenido)
        var buyerWallet = await _walletRepository.GetByUserIdAsync(buyerId, cancellationToken)
            ?? throw new NotFoundException($"No se encontró la billetera para el comprador {buyerId}.");

        decimal disponibleReal = buyerWallet.saldo_total - buyerWallet.saldo_retenido;

        if (disponibleReal < amount)
            throw new InsufficientFundsException();
    }
}