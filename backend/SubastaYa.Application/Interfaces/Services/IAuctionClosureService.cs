namespace SubastaYa.Application.Services;

/// <summary>
/// Servicio responsable ÚNICAMENTE de cerrar subastas expiradas.
/// </summary>
public interface IAuctionClosureService
{
    /// <summary>
    /// Cierra todas las subastas expiradas y procesa sus resultados.
    /// </summary>
    Task<int> CloseExpiredAuctionsAsync(CancellationToken cancellationToken = default);
}
