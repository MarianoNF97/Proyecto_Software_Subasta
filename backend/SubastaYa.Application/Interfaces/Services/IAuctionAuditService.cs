using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Interfaces.Services;

public interface IAuctionAuditService
{
    void LogAuctionClosedDeserted(int auctionId, DateTime fechaUtc);
    void LogAuctionClosedWithWinner(int auctionId, int winnerId, decimal amount, DateTime fechaUtc);
}