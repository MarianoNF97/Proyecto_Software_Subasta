using SubastaYa.Application.Interfaces.Repositories;
using SubastaYa.Domain.Constants;
using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Services;

/// <summary>
/// Implementación del servicio anti-sniping.
/// Responsabilidad única: extender tiempo de subasta cuando se oferta cerca del cierre.
/// </summary>
public class AntiSnipingService : IAntiSnipingService
{
    private readonly IAuctionRepository _auctionRepository;
    private readonly IAuditLogRepository _auditLogRepository;

    public AntiSnipingService(
        IAuctionRepository auctionRepository,
        IAuditLogRepository auditLogRepository)
    {
        _auctionRepository = auctionRepository;
        _auditLogRepository = auditLogRepository;
    }

    public async Task<bool> ApplyAntiSnipingRuleAsync(
        int auctionId,
        int bidderId,
        CancellationToken cancellationToken = default)
    {
        var subasta = await _auctionRepository.GetByIdWithBidsAsync(auctionId, cancellationToken);
        if (subasta == null) return false;

        var ahoraUtc = DateTime.UtcNow;
        var remainingTime = subasta.fecha_fin - ahoraUtc;

        if (remainingTime <= TimeSpan.FromSeconds(60))
        {
            subasta.fecha_fin = subasta.fecha_fin.AddMinutes(2);

            _auditLogRepository.Add(new AuditoriaLog
            {
                entidad = AuditConstants.ENTIDAD_SUBASTA,
                entidad_id = subasta.id,
                accion = AuditConstants.ACCION_EXTENSION_TIEMPO,
                usuario_id = bidderId,
                detalle_json = $"{{\"nuevaFechaFin\": \"{subasta.fecha_fin:O}\"}}",
                fecha = ahoraUtc
            });

            return true;
        }

        return false;
    }
}
