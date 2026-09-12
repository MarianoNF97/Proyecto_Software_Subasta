using Microsoft.Extensions.Logging;
using SubastaYa.Application.Interfaces;
using SubastaYa.Application.Interfaces.Repositories;
using SubastaYa.Application.Interfaces.Services;
using SubastaYa.Domain.Constants;

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
        _logger.LogInformation("Iniciando proceso de cierre de subastas expiradas.");
        var expiredAuctions = await _auctionRepository.GetExpiredActiveAuctionsAsync(cancellationToken);

        if (!expiredAuctions.Any())
        {
            _logger.LogInformation("No hay subastas expiradas para procesar.");
            return 0;
        }

        _logger.LogInformation("Encontradas {Count} subastas expiradas para cerrar.", expiredAuctions.Count());
        int closedCount = 0;

        foreach (var subasta in expiredAuctions)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

            string finalStatus;
            decimal finalPrice = 0;
            int? winnerId = null;

            try
            {
                var winningBid = _bidWinnerService.GetWinningBid(subasta.Pujas);
                var ahoraUtc = DateTime.UtcNow;

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
                closedCount++;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error al cerrar la subasta {AuctionId}.", subasta.id);
                throw;
            }

            await _notificationService.NotifyAuctionClosedAsync(
                subasta.id,
                finalStatus,
                finalPrice,
                winnerId,
                cancellationToken);
        }

        _logger.LogInformation("Proceso de cierre completado. Total cerradas: {ClosedCount}.", closedCount);
        return closedCount;
    }
}