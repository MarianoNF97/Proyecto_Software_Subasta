using SubastaYa.Application.Features.Auctions.Commands;
using SubastaYa.Application.Features.Auctions.Commands.Handlers;

namespace SubastaYa.Api.Workers;

public class AuctionActivationWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AuctionActivationWorker> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(10);

    public AuctionActivationWorker(IServiceProvider serviceProvider, ILogger<AuctionActivationWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AuctionActivationWorker iniciado correctamente.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<ActivateScheduledAuctionsHandler>();

                int activatedCount = await handler.HandleAsync(new ActivateScheduledAuctionsCommand(), stoppingToken);

                if (activatedCount > 0)
                {
                    _logger.LogInformation("Activación automática ejecutada: {Count} subastas pasaron a estado ACTIVA.", activatedCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el ciclo de ejecución del AuctionActivationWorker.");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }
}