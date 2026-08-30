using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Interfaces.Repositories;

public interface IAuditLogRepository
{
    void Add(AuditoriaLog log);
}