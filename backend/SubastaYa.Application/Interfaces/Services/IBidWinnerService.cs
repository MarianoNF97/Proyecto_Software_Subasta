using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Services;

public interface IBidWinnerService
{
    Puja? GetWinningBid(IEnumerable<Puja> bids);
}
