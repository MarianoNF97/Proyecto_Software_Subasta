using Microsoft.EntityFrameworkCore;
using SubastaYa.Application.Interfaces.Repositories;
using SubastaYa.Domain.Entities;

namespace SubastaYa.Infrastructure.Persistence.Repositories;

public class AuctionRepository : IAuctionRepository
{
    private readonly ApplicationDbContext _context;

    public AuctionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Subasta>> GetAllAsync(string? status, int? categoryId, CancellationToken cancellationToken = default)
    {
        var query = _context.Subastas
            .Include(s => s.Vendedor)
            .Include(s => s.Categoria)
            .Include(s => s.Pujas)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(s => s.estado == status.ToUpper());

        if (categoryId.HasValue)
            query = query.Where(s => s.categoria_id == categoryId.Value);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<Subasta?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Subastas
            .Include(s => s.Vendedor)
            .Include(s => s.Categoria)
            .Include(s => s.Pujas)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.id == id, cancellationToken);
    }

    public async Task<Subasta?> GetByIdWithBidsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Subastas
            .Include(s => s.Pujas)
            .FirstOrDefaultAsync(s => s.id == id, cancellationToken);
    }

    public async Task<IEnumerable<Subasta>> GetExpiredActiveAuctionsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Subastas
            .Include(s => s.Pujas)
            .Where(s => s.estado == "ACTIVA" && s.fecha_fin <= DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Subasta>> GetBySellerIdAsync(int sellerId, CancellationToken cancellationToken = default)
    {
        return await _context.Subastas
            .Include(s => s.Vendedor)
            .Include(s => s.Categoria)
            .Include(s => s.Pujas)
            .AsNoTracking()
            .Where(s => s.vendedor_id == sellerId)
            .OrderByDescending(s => s.fecha_inicio)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Subasta>> GetParticipatedByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.Subastas
            .Include(s => s.Vendedor)
            .Include(s => s.Categoria)
            .Include(s => s.Pujas)
            .AsNoTracking()
            .Where(s => s.Pujas.Any(p => p.comprador_id == userId))
            .OrderByDescending(s => s.fecha_fin)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Puja>> GetBidsByAuctionIdAsync(int auctionId, CancellationToken cancellationToken = default)
    {
        return await _context.Pujas
            .AsNoTracking()
            .Where(p => p.subasta_id == auctionId)
            .OrderByDescending(p => p.fecha_puja)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Subasta subasta, CancellationToken cancellationToken = default)
    {
        await _context.Subastas.AddAsync(subasta, cancellationToken);
    }
}