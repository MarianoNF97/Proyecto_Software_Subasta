using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Interfaces.Repositories;

public interface IAuctionRepository
{
    Task<IEnumerable<Subasta>> GetAllAsync(string? status, int? categoryId, CancellationToken cancellationToken = default);
    Task<Subasta?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<Subasta?> GetByIdWithBidsAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Subasta>> GetExpiredActiveAuctionsAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Subasta subasta, CancellationToken cancellationToken = default);
}