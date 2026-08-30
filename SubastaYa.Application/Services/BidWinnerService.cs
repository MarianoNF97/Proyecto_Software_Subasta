using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Services;

/// <summary>
/// Implementación del servicio para encontrar el ganador de una puja.
/// Responsabilidad única: determinar la puja ganadora según reglas de negocio.
/// </summary>
public class BidWinnerService : IBidWinnerService
{
    public Puja? GetWinningBid(IEnumerable<Puja> bids)
    {
        return bids.OrderByDescending(p => p.monto).FirstOrDefault();
    }
}
