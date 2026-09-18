using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Services;

public class BidWinnerService : IBidWinnerService
{
    public Puja? GetWinningBid(IEnumerable<Puja> bids)
    {
        return bids.OrderByDescending(p => p.monto).FirstOrDefault();
    }
}
