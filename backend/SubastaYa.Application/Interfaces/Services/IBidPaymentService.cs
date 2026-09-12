namespace SubastaYa.Application.Services;

/// <summary>
/// Servicio responsable ÚNICAMENTE de procesar pagos y gestión de fondos para pujas.
/// </summary>
public interface IBidPaymentService
{
    /// <summary>
    /// Procesa el pago de una puja: libera saldo del postor anterior y retiene del nuevo.
    /// </summary>
    Task ProcessBidPaymentAsync(
        int auctionId,
        int newBidderId,
        decimal amount,
        CancellationToken cancellationToken = default);
}
