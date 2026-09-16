using Microsoft.Extensions.Logging;
using SubastaYa.Application.Interfaces.Repositories;
using SubastaYa.Application.Interfaces.Services;
using SubastaYa.Domain.Constants;
using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Services;

public class AuctionClosureService : IAuctionClosureService
{
    private readonly IAuctionRepository _auctionRepository;
    private readonly IBidWinnerService _bidWinnerService;
    private readonly IAuctionSettlementService _settlementService;
    private readonly IAuctionAuditService _auditService;
    private readonly IAuctionNotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuctionClosureService> _logger;

    public AuctionClosureService(
        IAuctionRepository auctionRepository,
        IBidWinnerService bidWinnerService,
        IAuctionSettlementService settlementService,
        IAuctionAuditService auditService,
        IAuctionNotificationService notificationService,
        IUnitOfWork unitOfWork,
        ILogger<AuctionClosureService> logger)
    {
        _auctionRepository = auctionRepository;
        _bidWinnerService = bidWinnerService;
        _settlementService = settlementService;
        _auditService = auditService;
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<int> CloseExpiredAuctionsAsync(CancellationToken cancellationToken = default)
    {
        var expiredAuctions = await _auctionRepository.GetExpiredActiveAuctionsAsync(cancellationToken);
        var auctionsList = expiredAuctions.ToList();

        if (!auctionsList.Any())
        {
            return 0;
        }

        _logger.LogInformation("Encontradas {Count} subastas expiradas para procesar.", auctionsList.Count);
        int closedCount = 0;

        foreach (var subasta in auctionsList)
        {
            var success = await ProcessSingleAuctionClosureAsync(subasta, cancellationToken);
            if (success)
            {
                closedCount++;
            }
        }

        _logger.LogInformation("Proceso de cierre completado. Total cerradas exitosamente: {ClosedCount}.", closedCount);
        return closedCount;
    }

    private async Task<bool> ProcessSingleAuctionClosureAsync(Subasta subasta, CancellationToken cancellationToken)
    {
        using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        string finalStatus;
        decimal finalPrice = 0;
        int? winnerId = null;
        var ahoraUtc = DateTime.UtcNow;

        try
        {
            var winningBid = _bidWinnerService.GetWinningBid(subasta.Pujas);

            if (winningBid == null)
            {
                finalStatus = AuctionConstants.ESTADO_DESIERTA;
                subasta.estado = finalStatus;

                _auditService.LogAuctionClosedDeserted(subasta.id, ahoraUtc);
                _logger.LogInformation("Subasta {AuctionId} cerrada como DESIERTA.", subasta.id);
            }
            else
            {
                finalStatus = AuctionConstants.ESTADO_FINALIZADA;
                subasta.estado = finalStatus;
                finalPrice = winningBid.monto;
                winnerId = winningBid.comprador_id;

                await _settlementService.SettleAuctionPaymentAsync(subasta, winningBid, ahoraUtc, cancellationToken);
                _auditService.LogAuctionClosedWithWinner(subasta.id, winningBid.comprador_id, winningBid.monto, ahoraUtc);

                _logger.LogInformation("Subasta {AuctionId} cerrada como FINALIZADA. Ganador: {WinnerId}, Monto: {Amount}.",
                    subasta.id, winningBid.comprador_id, winningBid.monto);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Error transaccional al cerrar la subasta {AuctionId}.", subasta.id);
            return false;
        }

        // Notificación en tiempo real fuera de la transacción y protegida contra fallos de red
        try
        {
            await _notificationService.NotifyAuctionClosedAsync(
                subasta.id,
                finalStatus,
                finalPrice,
                winnerId,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "La subasta {AuctionId} fue cerrada en BD pero falló el envío de la notificación en tiempo real.", subasta.id);
        }

        return true;
    }
}