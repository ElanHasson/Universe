using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Configuration;
using System.Text.Json;
using System.Text.Json.Serialization;
using Universe.Abstractions.Grains;
using Universe.Abstractions.Visualization;
using Universe.Api.Services;
using Universe.Api.Hubs;
using Universe.Api.Converters;

var builder = WebApplication.CreateBuilder(args);

// Configure JSON serialization
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.SerializerOptions.Converters.Add(new Vector3JsonConverter());
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals;
});

// Add services
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Add Orleans client
builder.Services.AddOrleansClient(clientBuilder =>
{
    clientBuilder.UseLocalhostClustering()
        .Configure<ClusterOptions>(options =>
        {
            options.ClusterId = "dev";
            options.ServiceId = "Universe";
        });
});

builder.Services.AddSingleton<SimulationService>();
builder.Services.AddHostedService<SimulationUpdateService>();

// Configure connection retry through the builder
builder.Services.ConfigureAll<Orleans.Configuration.GatewayOptions>(options =>
{
    options.GatewayListRefreshPeriod = TimeSpan.FromSeconds(10);
});

// Add a simple health check endpoint
builder.Services.AddHealthChecks();

// Add SignalR for real-time updates (alternative to raw WebSockets)
builder.Services.AddSignalR()
    .AddJsonProtocol(options =>
    {
        options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.PayloadSerializerOptions.Converters.Add(new Vector3JsonConverter());
        options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.PayloadSerializerOptions.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals;
    });

var app = builder.Build();

app.UseCors("DevCors");

// WebSocket endpoint
app.UseWebSockets();

app.Map("/ws", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var simulationService = context.RequestServices.GetRequiredService<SimulationService>();
        await simulationService.HandleWebSocket(context);
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});

// SignalR hub (alternative)
app.MapHub<SimulationHub>("/simulationHub");

// REST API endpoints
app.MapGet("/api/snapshot", async (SimulationService service) => 
    await service.GetSnapshot());

app.MapPost("/api/particle", async (CreateParticleRequest request, SimulationService service) => 
    await service.CreateParticle(request));

app.MapPut("/api/particle/{id}", async (string id, UpdateParticleRequest request, SimulationService service) => 
    await service.UpdateParticle(id, request));

app.MapPost("/api/simulation/{command}", async (string command, SimulationService service) =>
{
    return command switch
    {
        "start" => await service.Start(),
        "pause" => await service.Pause(),
        "reset" => await service.Reset(),
        _ => Results.BadRequest("Unknown command")
    };
});

// Add health check endpoint
app.MapHealthChecks("/health");

app.Run("http://localhost:5000");

// Request/Response DTOs
public record CreateParticleRequest(string Type, Vector3Dto Position);
public record UpdateParticleRequest(Vector3Dto? Position, Vector3Dto? Momentum, double? Energy, double? Spin);
public record Vector3Dto(double X, double Y, double Z);