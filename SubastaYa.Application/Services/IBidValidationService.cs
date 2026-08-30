namespace SubastaYa.Application.Services;

/// <summary>
/// Servicio responsable ÚNICAMENTE de validar reglas de negocio para pujas.
/// </summary>
public interface IBidValidationService
{
    /// <summary>
    /// Valida que una puja sea válida según las reglas de negocio.
    /// </summary>
    Task ValidateBidAsync(
        int auctionId,
        int buyerId,
        decimal amount,
        CancellationToken cancellationToken = default);
}
