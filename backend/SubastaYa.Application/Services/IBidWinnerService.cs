using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Services;

/// <summary>
/// Servicio responsable ÚNICAMENTE de determinar la puja ganadora.
/// </summary>
public interface IBidWinnerService
{
    /// <summary>
    /// Obtiene la puja ganadora (la de mayor monto) de una colección de pujas.
    /// </summary>
    Puja? GetWinningBid(IEnumerable<Puja> bids);
}
