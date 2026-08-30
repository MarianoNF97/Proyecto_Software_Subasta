namespace SubastaYa.Application.Services;

/// <summary>
/// Servicio responsable ÚNICAMENTE de procesar depósitos en billeteras.
/// </summary>
public interface IDepositService
{
    /// <summary>
    /// Procesa un depósito de fondos a la billetera de un usuario.
    /// </summary>
    Task<bool> DepositFundsAsync(int userId, decimal amount, CancellationToken cancellationToken = default);
}
