using Microsoft.AspNetCore.SignalR;
using Universe.Api.Services;

namespace Universe.Api.Hubs;

public class SimulationHub : Hub
{
    private readonly SimulationService _simulationService;
    private readonly ILogger<SimulationHub> _logger;

    public SimulationHub(SimulationService simulationService, ILogger<SimulationHub> logger)
    {
        _simulationService = simulationService;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        
        // Send initial snapshot
        var snapshot = await _simulationService.GetSnapshot();
        await Clients.Caller.SendAsync("snapshot", snapshot);
        
        await base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }

    public async Task GetSnapshot()
    {
        var snapshot = await _simulationService.GetSnapshot();
        await Clients.Caller.SendAsync("snapshot", snapshot);
    }

    public async Task CreateParticle(string type, double x, double y, double z)
    {
        await _simulationService.CreateParticle(new CreateParticleRequest(
            type, 
            new Vector3Dto(x, y, z)));
        await _simulationService.BroadcastSnapshot();
    }

    public async Task UpdateParticle(string particleId, UpdateParticleRequest updates)
    {
        await _simulationService.UpdateParticle(particleId, updates);
        await _simulationService.BroadcastSnapshot();
    }

    public async Task Start()
    {
        await _simulationService.Start();
    }

    public async Task Pause()
    {
        await _simulationService.Pause();
    }

    public async Task Reset()
    {
        await _simulationService.Reset();
        await _simulationService.BroadcastSnapshot();
    }
}