using SubastaYa.Application.Common.Interfaces;
using SubastaYa.Application.Features.Auctions.Commands;
using SubastaYa.Application.Interfaces;
using SubastaYa.Application.Interfaces.Services;
using SubastaYa.Application.Services;

namespace SubastaYa.Application.Features.Auctions.Commands.Handlers;

/// <summary>
/// Handler responsable ÚNICAMENTE de orquestar el cierre de subastas expiradas.
/// Delega toda la lógica al servicio especializado.
/// </summary>
public class CloseExpiredAuctionsHandler : ICommandHandler<CloseExpiredAuctionsCommand, int>
{
    private readonly IAuctionClosureService _auctionClosureService;

    public CloseExpiredAuctionsHandler(IAuctionClosureService auctionClosureService)
    {
        _auctionClosureService = auctionClosureService;
    }

    public async Task<int> HandleAsync(CloseExpiredAuctionsCommand command, CancellationToken cancellationToken = default)
    {
        return await _auctionClosureService.CloseExpiredAuctionsAsync(cancellationToken);
    }
}