using SubastaYa.Application.Features.Auctions.Commands;
using SubastaYa.Application.Features.Auctions.Commands.Handlers;

namespace SubastaYa.Api.Workers;

public class AuctionClosingWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AuctionClosingWorker> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(10);

    public AuctionClosingWorker(IServiceProvider serviceProvider, ILogger<AuctionClosingWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AuctionClosingWorker iniciado correctamente.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<CloseExpiredAuctionsHandler>();

                int closedCount = await handler.HandleAsync(new CloseExpiredAuctionsCommand(), stoppingToken);

                if (closedCount > 0)
                {
                    _logger.LogInformation("Cierre automático ejecutado: {Count} subastas procesadas y finalizadas.", closedCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el ciclo de ejecución del AuctionClosingWorker.");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }
}