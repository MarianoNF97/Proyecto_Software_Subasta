using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Interfaces.Repositories;

public interface IAuctionRepository
{
    Task<IEnumerable<Subasta>> GetAllAsync(string? status, int? categoryId, decimal? minPrice = null, decimal? maxPrice = null, CancellationToken cancellationToken = default);
    Task<Subasta?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<Subasta?> GetByIdWithBidsAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Subasta>> GetExpiredActiveAuctionsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Subasta>> GetScheduledAuctionsToActivateAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Subasta subasta, CancellationToken cancellationToken = default);
    Task<IEnumerable<Subasta>> GetBySellerIdAsync(int sellerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Subasta>> GetParticipatedByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Puja>> GetBidsByAuctionIdAsync(int auctionId, CancellationToken cancellationToken = default);
}