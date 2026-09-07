using Microsoft.Extensions.Logging;
using SubastaYa.Application.Interfaces.Repositories;
using SubastaYa.Domain.Constants;
using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Services;

/// <summary>
/// Implementación del servicio de cierre de subastas.
/// Responsabilidad única: cerrar subastas expiradas y procesar pagos.
/// </summary>
public class AuctionClosureService : IAuctionClosureService
{
    private readonly IAuctionRepository _auctionRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IBidWinnerService _bidWinnerService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuctionClosureService> _logger;

    public AuctionClosureService(
        IAuctionRepository auctionRepository,
        IWalletRepository walletRepository,
        IAuditLogRepository auditLogRepository,
        IBidWinnerService bidWinnerService,
        IUnitOfWork unitOfWork,
        ILogger<AuctionClosureService> logger)
    {
        _auctionRepository = auctionRepository;
        _walletRepository = walletRepository;
        _auditLogRepository = auditLogRepository;
        _bidWinnerService = bidWinnerService;
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

            try
            {
                var winningBid = _bidWinnerService.GetWinningBid(subasta.Pujas);
                var ahoraUtc = DateTime.UtcNow;

                if (winningBid == null)
                {
                    subasta.estado = AuctionConstants.ESTADO_DESIERTA;

                    _auditLogRepository.Add(new AuditoriaLog
                    {
                        entidad = AuditConstants.ENTIDAD_SUBASTA,
                        entidad_id = subasta.id,
                        accion = AuditConstants.ACCION_CIERRE_DESIERTA,
                        usuario_id = null,
                        detalle_json = "{\"mensaje\": \"Subasta finalizada sin ofertas.\"}",
                        fecha = ahoraUtc
                    });

                    _logger.LogInformation("Subasta {AuctionId} cerrada como DESIERTA.", subasta.id);
                }
                else
                {
                    subasta.estado = AuctionConstants.ESTADO_FINALIZADA;

                    var buyerWallet = await _walletRepository.GetByUserIdAsync(winningBid.comprador_id, cancellationToken);
                    if (buyerWallet != null)
                    {
                        buyerWallet.saldo_retenido -= winningBid.monto;
                        buyerWallet.saldo_total -= winningBid.monto;

                        _walletRepository.AddTransaction(new TransaccionLedger
                        {
                            billetera_id = buyerWallet.id,
                            tipo = TransactionConstants.TIPO_PAGO,
                            monto = winningBid.monto,
                            fecha = ahoraUtc,
                            subasta_id = subasta.id
                        });
                    }

                    var sellerWallet = await _walletRepository.GetByUserIdAsync(subasta.vendedor_id, cancellationToken);
                    if (sellerWallet != null)
                    {
                        sellerWallet.saldo_total += winningBid.monto;

                        _walletRepository.AddTransaction(new TransaccionLedger
                        {
                            billetera_id = sellerWallet.id,
                            tipo = TransactionConstants.TIPO_COBRO,
                            monto = winningBid.monto,
                            fecha = ahoraUtc,
                            subasta_id = subasta.id
                        });
                    }

                    _auditLogRepository.Add(new AuditoriaLog
                    {
                        entidad = AuditConstants.ENTIDAD_SUBASTA,
                        entidad_id = subasta.id,
                        accion = AuditConstants.ACCION_CIERRE_FINALIZADA,
                        usuario_id = null,
                        detalle_json = $"{{\"ganadorId\": {winningBid.comprador_id}, \"monto\": {winningBid.monto}}}",
                        fecha = ahoraUtc
                    });

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
        }

        _logger.LogInformation("Proceso de cierre completado. Total cerradas: {ClosedCount}.", closedCount);
        return closedCount;
    }
}
