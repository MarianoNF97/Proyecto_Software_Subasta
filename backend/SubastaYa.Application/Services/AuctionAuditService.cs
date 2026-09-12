using SubastaYa.Application.Interfaces.Repositories;
using SubastaYa.Application.Interfaces.Services;
using SubastaYa.Domain.Constants;
using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Services;

public class AuctionAuditService : IAuctionAuditService
{
    private readonly IAuditLogRepository _auditLogRepository;

    public AuctionAuditService(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public void LogAuctionClosedDeserted(int auctionId, DateTime fechaUtc)
    {
        _auditLogRepository.Add(new AuditoriaLog
        {
            entidad = AuditConstants.ENTIDAD_SUBASTA,
            entidad_id = auctionId,
            accion = AuditConstants.ACCION_CIERRE_DESIERTA,
            usuario_id = null,
            detalle_json = "{\"mensaje\": \"Subasta finalizada sin ofertas.\"}",
            fecha = fechaUtc
        });
    }

    public void LogAuctionClosedWithWinner(int auctionId, int winnerId, decimal amount, DateTime fechaUtc)
    {
        _auditLogRepository.Add(new AuditoriaLog
        {
            entidad = AuditConstants.ENTIDAD_SUBASTA,
            entidad_id = auctionId,
            accion = AuditConstants.ACCION_CIERRE_FINALIZADA,
            usuario_id = null,
            detalle_json = $"{{\"ganadorId\": {winnerId}, \"monto\": {amount}}}",
            fecha = fechaUtc
        });
    }
}