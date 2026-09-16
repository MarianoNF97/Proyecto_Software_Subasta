using SubastaYa.Application.Features.Auctions.Commands;
using SubastaYa.Application.Interfaces;
using SubastaYa.Application.Interfaces.Services;

namespace SubastaYa.Application.Features.Auctions.Commands.Handlers;

/// <summary>
/// Handler responsable ÚNICAMENTE de orquestar la activación de subastas programadas.
/// Delega toda la lógica al servicio especializado.
/// </summary>
public class ActivateScheduledAuctionsHandler : ICommandHandler<ActivateScheduledAuctionsCommand, int>
{
    private readonly IAuctionActivationService _auctionActivationService;

    public ActivateScheduledAuctionsHandler(IAuctionActivationService auctionActivationService)
    {
        _auctionActivationService = auctionActivationService;
    }

    public async Task<int> HandleAsync(ActivateScheduledAuctionsCommand command, CancellationToken cancellationToken = default)
    {
        return await _auctionActivationService.ActivateScheduledAuctionsAsync(cancellationToken);
    }
}