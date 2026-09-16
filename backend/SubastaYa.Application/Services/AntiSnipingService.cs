using SubastaYa.Application.Interfaces.Repositories;
using SubastaYa.Application.Interfaces.Services;
using SubastaYa.Domain.Constants;
using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Services;

/// <summary>
/// Implementación del servicio anti-sniping.
/// Responsabilidad única: extender 2 minutos la subasta si se oferta en los últimos 60 segundos.
/// </summary>
public class AntiSnipingService : IAntiSnipingService
{
    private readonly IAuditLogRepository _auditLogRepository;
    private static readonly TimeSpan CriticalWindow = TimeSpan.FromSeconds(60); // 60 segundos oficiales
    private static readonly TimeSpan ExtensionTime = TimeSpan.FromMinutes(2);   // 2 minutos de extensión

    public AntiSnipingService(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public bool ApplyAntiSnipingRule(Subasta subasta, int bidderId, DateTime ahoraUtc)
    {
        var remainingTime = subasta.fecha_fin - ahoraUtc;

        if (remainingTime > TimeSpan.Zero && remainingTime <= CriticalWindow)
        {
            subasta.fecha_fin = subasta.fecha_fin.Add(ExtensionTime);

            _auditLogRepository.Add(new AuditoriaLog
            {
                entidad = AuditConstants.ENTIDAD_SUBASTA,
                entidad_id = subasta.id,
                accion = AuditConstants.ACCION_EXTENSION_TIEMPO,
                usuario_id = bidderId,
                detalle_json = $"{{\"nuevaFechaFin\": \"{subasta.fecha_fin:O}\", \"minutosAgregados\": 2}}",
                fecha = ahoraUtc
            });

            return true;
        }

        return false;
    }
}