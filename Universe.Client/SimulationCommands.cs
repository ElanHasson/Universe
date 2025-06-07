using Microsoft.Extensions.Logging;
using Orleans;
using System.Numerics;
using Universe.Abstractions.Grains;
using Universe.Abstractions.Physics;
using Universe.Abstractions.Visualization;
using Universe.Grains;

namespace Universe.Client;

public class SimulationCommands
{
    private readonly IClusterClient _client;
    private readonly ILogger<SimulationCommands> _logger;
    private readonly ISimulationOrchestratorGrain _orchestrator;
    private readonly IVisualizationService _visualization;

    public SimulationCommands(IClusterClient client, ILogger<SimulationCommands> logger, ILoggerFactory loggerFactory)
    {
        _client = client;
        _logger = logger;
        _orchestrator = _client.GetGrain<ISimulationOrchestratorGrain>(0);
        _visualization = new VisualizationService(client, loggerFactory.CreateLogger<VisualizationService>());
    }

    public async Task CreateProton(Vector3 position)
    {
        try
        {
            var quarks = await _orchestrator.CreateBaryon(
                new[] { QuarkFlavor.Up, QuarkFlavor.Up, QuarkFlavor.Down },
                position);
            
            Console.WriteLine($"Created proton with quarks: {string.Join(", ", quarks)}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create proton");
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    public async Task CreateNeutron(Vector3 position)
    {
        try
        {
            var quarks = await _orchestrator.CreateBaryon(
                new[] { QuarkFlavor.Up, QuarkFlavor.Down, QuarkFlavor.Down },
                position);
            
            Console.WriteLine($"Created neutron with quarks: {string.Join(", ", quarks)}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create neutron");
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    public async Task CreatePion(string type, Vector3 position)
    {
        try
        {
            List<Guid> quarks;
            switch (type.ToLower())
            {
                case "positive":
                    quarks = await _orchestrator.CreateMeson(QuarkFlavor.Up, QuarkFlavor.Down, position);
                    Console.WriteLine($"Created π+ meson with quarks: {string.Join(", ", quarks)}");
                    break;
                case "negative":
                    quarks = await _orchestrator.CreateMeson(QuarkFlavor.Down, QuarkFlavor.Up, position);
                    Console.WriteLine($"Created π- meson with quarks: {string.Join(", ", quarks)}");
                    break;
                case "neutral":
                    quarks = await _orchestrator.CreateQuarkPair(QuarkFlavor.Up, position);
                    Console.WriteLine($"Created π0 meson with quarks: {string.Join(", ", quarks)}");
                    break;
                default:
                    Console.WriteLine("Invalid pion type. Use: positive, negative, or neutral");
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create pion");
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    public async Task ShowStats()
    {
        try
        {
            var stats = await _orchestrator.GetSimulationStats();
            
            Console.WriteLine("\n=== Simulation Statistics ===");
            Console.WriteLine($"Simulation Time: {stats.SimulationTime:F3} seconds");
            Console.WriteLine($"Total Quarks: {stats.TotalQuarks}");
            Console.WriteLine($"  - Free: {stats.FreeeQuarks}");
            Console.WriteLine($"  - Confined: {stats.ConfinedQuarks}");
            Console.WriteLine($"Hadrons:");
            Console.WriteLine($"  - Mesons: {stats.Mesons}");
            Console.WriteLine($"  - Baryons: {stats.Baryons}");
            Console.WriteLine($"Total Energy: {stats.TotalEnergy:F3} GeV");
            Console.WriteLine($"Total Momentum: {stats.TotalMomentum:F3} GeV/c");
            Console.WriteLine($"Active Interactions: {stats.InteractionCount}");
            Console.WriteLine("=============================\n");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get statistics");
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    public async Task Start()
    {
        try
        {
            await _orchestrator.Resume();
            Console.WriteLine("Simulation started");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start simulation");
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    public async Task Pause()
    {
        try
        {
            await _orchestrator.Pause();
            Console.WriteLine("Simulation paused");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to pause simulation");
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    public async Task Reset()
    {
        try
        {
            await _orchestrator.Reset();
            Console.WriteLine("Simulation reset");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reset simulation");
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    public async Task ListQuarks()
    {
        try
        {
            var quarks = await _orchestrator.GetAllQuarks();
            
            Console.WriteLine("\n=== Quarks in Simulation ===");
            foreach (var quark in quarks)
            {
                var confinementStatus = quark.BoundPartners.Any() ? "Confined" : "Free";
                var charge = quark.ElectricCharge > 0 ? $"+{quark.ElectricCharge:F2}" : $"{quark.ElectricCharge:F2}";
                
                Console.WriteLine($"ID: {quark.QuarkId:N}");
                Console.WriteLine($"  Flavor: {quark.Flavor}{(quark.IsAntiParticle ? " (anti)" : "")}");
                Console.WriteLine($"  Color: {quark.Color}");
                Console.WriteLine($"  Charge: {charge}e");
                Console.WriteLine($"  Mass: {quark.Mass:F3} GeV/c²");
                Console.WriteLine($"  Position: ({quark.Position.X:F3}, {quark.Position.Y:F3}, {quark.Position.Z:F3})");
                Console.WriteLine($"  Momentum: {quark.Momentum.Length():F3} GeV/c");
                Console.WriteLine($"  Spin: {(quark.SpinZ > 0 ? "↑" : "↓")} ({quark.SpinZ})");
                Console.WriteLine($"  Status: {confinementStatus}");
                
                if (quark.BoundPartners.Any())
                {
                    Console.WriteLine($"  Partners: {string.Join(", ", quark.BoundPartners.Select(p => p.ToString("N")))}");
                }
                
                Console.WriteLine();
            }
            Console.WriteLine($"Total: {quarks.Count} quarks");
            Console.WriteLine("============================\n");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list quarks");
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    public async Task Visualize()
    {
        try
        {
            var snapshot = await _visualization.CaptureSnapshot();
            var ascii = await _visualization.GenerateASCIIVisualization(snapshot);
            
            Console.Clear();
            Console.WriteLine(ascii);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate visualization");
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    public async Task SaveSnapshot(string filename)
    {
        try
        {
            var snapshot = await _visualization.CaptureSnapshot();
            await _visualization.SaveSnapshotToFile(snapshot, filename);
            Console.WriteLine($"Snapshot saved to {filename}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save snapshot");
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    public async Task Monitor(int intervalSeconds = 1)
    {
        try
        {
            Console.WriteLine("Starting real-time monitoring. Press Q to quit.");
            
            var cts = new CancellationTokenSource();
            var monitorTask = Task.Run(async () =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        var snapshot = await _visualization.CaptureSnapshot();
                        var ascii = await _visualization.GenerateASCIIVisualization(snapshot);
                        
                        Console.SetCursorPosition(0, 0);
                        Console.WriteLine(ascii);
                        
                        await Task.Delay(intervalSeconds * 1000, cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            });
            
            // Wait for user to press Q
            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Q)
                {
                    cts.Cancel();
                    break;
                }
            }
            
            await monitorTask;
            Console.WriteLine("\nMonitoring stopped.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to monitor simulation");
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}