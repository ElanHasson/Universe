using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Universe.Api.Services;

public class SimulationUpdateService : BackgroundService
{
    private readonly SimulationService _simulationService;
    private readonly ILogger<SimulationUpdateService> _logger;
    private readonly TimeSpan _updateInterval = TimeSpan.FromMilliseconds(100); // 10 FPS

    public SimulationUpdateService(
        SimulationService simulationService,
        ILogger<SimulationUpdateService> logger)
    {
        _simulationService = simulationService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Simulation update service starting");
        
        // Wait a bit for Orleans to connect
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        
        _logger.LogInformation("Simulation update service starting broadcast loop");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _simulationService.BroadcastSnapshot();
                _logger.LogInformation("Broadcasted simulation snapshot");
                await Task.Delay(_updateInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
                break;
            }
            catch (Orleans.Runtime.OrleansException)
            {
                // Orleans not connected yet, just skip this update
                _logger.LogDebug("Orleans not connected, skipping update");
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting simulation update");
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }

        _logger.LogInformation("Simulation update service stopping");
    }
}