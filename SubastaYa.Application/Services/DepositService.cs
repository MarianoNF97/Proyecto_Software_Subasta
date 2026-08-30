using SubastaYa.Application.Exceptions;
using SubastaYa.Application.Interfaces.Repositories;
using SubastaYa.Domain.Constants;
using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Services;

/// <summary>
/// Implementación del servicio de depósitos.
/// Responsabilidad única: procesar depósitos de fondos en billeteras.
/// </summary>
public class DepositService : IDepositService
{
    private readonly IWalletRepository _walletRepository;
    private readonly IAuditLogRepository _auditLogRepository;

    public DepositService(
        IWalletRepository walletRepository,
        IAuditLogRepository auditLogRepository)
    {
        _walletRepository = walletRepository;
        _auditLogRepository = auditLogRepository;
    }

    public async Task<bool> DepositFundsAsync(int userId, decimal amount, CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
            throw new BusinessValidationException("El monto a depositar debe ser mayor a cero.");

        var wallet = await _walletRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException($"No se encontró la billetera para el usuario con ID {userId}.");

        wallet.saldo_total += amount;

        _walletRepository.AddTransaction(new TransaccionLedger
        {
            billetera_id = wallet.id,
            tipo = TransactionConstants.TIPO_DEPOSITO,
            monto = amount,
            fecha = DateTime.UtcNow
        });

        _auditLogRepository.Add(new AuditoriaLog
        {
            entidad = AuditConstants.ENTIDAD_BILLETERA,
            entidad_id = wallet.id,
            accion = AuditConstants.ACCION_CARGA_SALDO,
            usuario_id = userId,
            detalle_json = $"{{\"monto\": {amount}}}",
            fecha = DateTime.UtcNow
        });

        return true;
    }
}
