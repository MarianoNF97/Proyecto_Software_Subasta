namespace SubastaYa.Application.Services;

/// <summary>
/// Servicio responsable ÚNICAMENTE de aplicar la regla anti-sniping.
/// </summary>
public interface IAntiSnipingService
{
    /// <summary>
    /// Aplica la regla anti-sniping: extiende tiempo si falta menos de 60 segundos.
    /// </summary>
    Task<bool> ApplyAntiSnipingRuleAsync(
        int auctionId,
        int bidderId,
        CancellationToken cancellationToken = default);
}
