using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Interfaces.Repositories;

public interface IWalletRepository
{
    Task<Billetera?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TransaccionLedger>> GetTransactionsByWalletIdAsync(int walletId, CancellationToken cancellationToken = default);
    void AddTransaction(TransaccionLedger transaction);
}