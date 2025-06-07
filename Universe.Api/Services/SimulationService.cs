using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using Universe.Abstractions.Grains;
using Universe.Abstractions.Physics;
using Universe.Abstractions.Visualization;
using Universe.Api.Converters;
using Universe.Grains;

namespace Universe.Api.Services;

public class SimulationService
{
    private readonly IClusterClient _client;
    private readonly IVisualizationService _visualization;
    private readonly ILogger<SimulationService> _logger;
    private readonly List<WebSocket> _connectedClients = new();
    private readonly SemaphoreSlim _clientLock = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions;

    public SimulationService(IClusterClient client, ILogger<SimulationService> logger, ILoggerFactory loggerFactory)
    {
        _client = client;
        _logger = logger;
        _visualization = new VisualizationService(client, loggerFactory.CreateLogger<VisualizationService>());
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals | JsonNumberHandling.AllowReadingFromString,
            Converters = 
            {
                new JsonStringEnumConverter(),
                new Vector3JsonConverter()
            }
        };
    }

    public async Task HandleWebSocket(HttpContext context)
    {
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        
        await _clientLock.WaitAsync();
        try
        {
            _connectedClients.Add(webSocket);
            _logger.LogInformation("WebSocket client connected. Total clients: {Count}", _connectedClients.Count);
        }
        finally
        {
            _clientLock.Release();
        }

        var buffer = new byte[1024 * 4];
        
        try
        {
            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    await HandleMessage(webSocket, message);
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, 
                        "Closing", CancellationToken.None);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "WebSocket error");
        }
        finally
        {
            await _clientLock.WaitAsync();
            try
            {
                _connectedClients.Remove(webSocket);
                _logger.LogInformation("WebSocket client disconnected. Total clients: {Count}", 
                    _connectedClients.Count);
            }
            finally
            {
                _clientLock.Release();
            }
        }
    }

    private async Task HandleMessage(WebSocket webSocket, string message)
    {
        try
        {
            _logger.LogInformation("Received WebSocket message: {Message}", message);
            var request = JsonSerializer.Deserialize<WebSocketMessage>(message);
            if (request == null) 
            {
                _logger.LogWarning("Failed to deserialize WebSocket message");
                return;
            }

            _logger.LogInformation("Processing message type: {Type}", request.Type);
            switch (request.Type)
            {
                case "getSnapshot":
                    var snapshot = await GetSnapshot();
                    await SendMessage(webSocket, "snapshot", snapshot);
                    break;

                case "createParticle":
                    if (request.Payload.HasValue)
                    {
                        _logger.LogInformation("Processing createParticle with payload");
                        var createRequest = JsonSerializer.Deserialize<CreateParticleRequest>(
                            request.Payload.Value.GetRawText(), _jsonOptions);
                        if (createRequest != null)
                        {
                            _logger.LogInformation("Creating particle type: {Type} at position: {Position}", 
                                createRequest.Type, createRequest.Position);
                            var result = await CreateParticle(createRequest);
                            if (result is not BadRequestObjectResult)
                            {
                                _logger.LogInformation("Particle created successfully");
                                await BroadcastSnapshot();
                            }
                            else
                            {
                                _logger.LogWarning("Particle creation failed");
                            }
                        }
                        else
                        {
                            _logger.LogWarning("Failed to deserialize CreateParticleRequest");
                        }
                    }
                    else
                    {
                        _logger.LogWarning("createParticle message has no payload");
                    }
                    break;

                case "updateParticle":
                    if (request.Payload.HasValue)
                    {
                        var updateRequest = JsonSerializer.Deserialize<UpdateParticleMessage>(
                            request.Payload.Value.GetRawText());
                        if (updateRequest != null)
                        {
                            await UpdateParticle(updateRequest.ParticleId, updateRequest.Updates);
                            await BroadcastSnapshot();
                        }
                    }
                    break;

                case "pause":
                    await Pause();
                    break;

                case "resume":
                    await Start();
                    break;

                case "reset":
                    await Reset();
                    await BroadcastSnapshot();
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling WebSocket message");
            await SendMessage(webSocket, "error", new { message = ex.Message });
        }
    }

    private async Task SendMessage(WebSocket webSocket, string type, object payload)
    {
        if (webSocket.State != WebSocketState.Open) return;

        var message = new
        {
            type,
            payload
        };

        var json = JsonSerializer.Serialize(message, _jsonOptions);
        var bytes = Encoding.UTF8.GetBytes(json);

        await webSocket.SendAsync(
            new ArraySegment<byte>(bytes),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None);
    }

    public async Task BroadcastSnapshot()
    {
        var snapshot = await GetSnapshot();
        
        await _clientLock.WaitAsync();
        try
        {
            var openClients = _connectedClients
                .Where(ws => ws.State == WebSocketState.Open)
                .ToList();
                
            _logger.LogInformation("Broadcasting snapshot to {Count} connected clients", openClients.Count);
            
            var tasks = openClients
                .Select(ws => SendMessage(ws, "snapshot", snapshot))
                .ToList();
            
            await Task.WhenAll(tasks);
        }
        finally
        {
            _clientLock.Release();
        }
    }

    public async Task<SimulationSnapshot> GetSnapshot()
    {
        return await _visualization.CaptureSnapshot();
    }

    public async Task<IResult> CreateParticle(CreateParticleRequest request)
    {
        try
        {
            _logger.LogInformation("CreateParticle called with type: {Type}, position: ({X}, {Y}, {Z})", 
                request.Type, request.Position.X, request.Position.Y, request.Position.Z);
                
            var orchestrator = _client.GetGrain<ISimulationOrchestratorGrain>(0);
            var position = new System.Numerics.Vector3(
                (float)request.Position.X, 
                (float)request.Position.Y, 
                (float)request.Position.Z);

            switch (request.Type.ToLower())
            {
            case "proton":
                await orchestrator.CreateBaryon(
                    new[] { QuarkFlavor.Up, QuarkFlavor.Up, QuarkFlavor.Down }, 
                    position);
                break;
                
            case "neutron":
                await orchestrator.CreateBaryon(
                    new[] { QuarkFlavor.Up, QuarkFlavor.Down, QuarkFlavor.Down }, 
                    position);
                break;
                
            case "pion":
                await orchestrator.CreateMeson(QuarkFlavor.Up, QuarkFlavor.Down, position);
                break;
                
            case "quark":
                await orchestrator.CreateQuark(
                    QuarkFlavor.Up, 
                    ColorCharge.Red, 
                    false, 
                    position, 
                    System.Numerics.Vector3.Zero, 
                    0.5);
                break;
                
            default:
                _logger.LogWarning("Unknown particle type: {Type}", request.Type);
                return Results.BadRequest($"Unknown particle type: {request.Type}");
        }

        _logger.LogInformation("Particle creation completed successfully");
        return Results.Ok();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error creating particle");
        return Results.Problem(ex.Message);
    }
    }

    public async Task<IResult> UpdateParticle(string particleId, UpdateParticleRequest request)
    {
        if (!Guid.TryParse(particleId, out var id))
            return Results.BadRequest("Invalid particle ID");

        var quark = _client.GetGrain<IQuarkGrain>(id);
        
        if (request.Position != null)
        {
            await quark.UpdatePosition(new System.Numerics.Vector3(
                (float)request.Position.X,
                (float)request.Position.Y,
                (float)request.Position.Z));
        }

        if (request.Momentum != null)
        {
            await quark.UpdateMomentum(new System.Numerics.Vector3(
                (float)request.Momentum.X,
                (float)request.Momentum.Y,
                (float)request.Momentum.Z));
        }

        if (request.Energy != null)
        {
            await quark.UpdateEnergy(request.Energy.Value);
        }

        if (request.Spin != null)
        {
            await quark.UpdateSpin(request.Spin.Value);
        }

        return Results.Ok();
    }

    public async Task<IResult> Start()
    {
        var orchestrator = _client.GetGrain<ISimulationOrchestratorGrain>(0);
        await orchestrator.Resume();
        return Results.Ok();
    }

    public async Task<IResult> Pause()
    {
        var orchestrator = _client.GetGrain<ISimulationOrchestratorGrain>(0);
        await orchestrator.Pause();
        return Results.Ok();
    }

    public async Task<IResult> Reset()
    {
        var orchestrator = _client.GetGrain<ISimulationOrchestratorGrain>(0);
        await orchestrator.Reset();
        return Results.Ok();
    }
}

public class WebSocketMessage
{
    public string Type { get; set; } = "";
    public JsonElement? Payload { get; set; }
}

public class UpdateParticleMessage
{
    public string ParticleId { get; set; } = "";
    public UpdateParticleRequest Updates { get; set; } = new(null, null, null, null);
}