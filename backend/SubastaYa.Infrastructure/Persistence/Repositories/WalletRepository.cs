using Microsoft.EntityFrameworkCore;
using SubastaYa.Application.Interfaces.Repositories;
using SubastaYa.Domain.Entities;

namespace SubastaYa.Infrastructure.Persistence.Repositories;

public class WalletRepository : IWalletRepository
{
    private readonly ApplicationDbContext _context;

    public WalletRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Billetera?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.Billeteras
            .FirstOrDefaultAsync(b => b.usuario_id == userId, cancellationToken);
    }

    public async Task<IEnumerable<TransaccionLedger>> GetTransactionsByWalletIdAsync(int walletId, CancellationToken cancellationToken = default)
    {
        return await _context.TransaccionesLedger
            .AsNoTracking()
            .Where(t => t.billetera_id == walletId)
            .OrderByDescending(t => t.fecha)
            .ToListAsync(cancellationToken);
    }

    public void AddTransaction(TransaccionLedger transaction)
    {
        _context.TransaccionesLedger.Add(transaction);
    }
}