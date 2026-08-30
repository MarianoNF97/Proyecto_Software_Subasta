using SubastaYa.Application.Features.Wallets.Commands;
using SubastaYa.Application.Interfaces;
using SubastaYa.Application.Interfaces.Repositories;
using SubastaYa.Application.Services;

namespace SubastaYa.Application.Features.Wallets.Commands.Handlers;

/// <summary>
/// Handler responsable ÚNICAMENTE de orquestar depósitos de fondos.
/// Delega toda la lógica al servicio especializado.
/// </summary>
public class DepositFundsHandler : ICommandHandler<DepositFundsCommand, bool>
{
    private readonly IDepositService _depositService;
    private readonly IUnitOfWork _unitOfWork;

    public DepositFundsHandler(
        IDepositService depositService,
        IUnitOfWork unitOfWork)
    {
        _depositService = depositService;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> HandleAsync(DepositFundsCommand command, CancellationToken cancellationToken = default)
    {
        var result = await _depositService.DepositFundsAsync(command.UserId, command.Amount, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return result;
    }
}