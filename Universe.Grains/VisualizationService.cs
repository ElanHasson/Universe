using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Orleans;
using Universe.Abstractions.Grains;
using Universe.Abstractions.Physics;
using Universe.Abstractions.Visualization;

namespace Universe.Grains;

public class VisualizationService : IVisualizationService
{
    private readonly IClusterClient _client;
    private readonly ILogger<VisualizationService> _logger;
    private readonly List<SimulationSnapshot> _recentSnapshots = new();
    private const int MaxSnapshots = 100;

    public VisualizationService(IClusterClient client, ILogger<VisualizationService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<SimulationSnapshot> CaptureSnapshot()
    {
        var orchestrator = _client.GetGrain<ISimulationOrchestratorGrain>(0);
        var stats = await orchestrator.GetSimulationStats();
        var quarks = await orchestrator.GetAllQuarks();
        var interactions = await orchestrator.GetActiveInteractions();
        
        var particles = new List<ParticleSnapshot>();
        var interactionSnapshots = new List<InteractionSnapshot>();
        var fields = new List<FieldSnapshot>();
        
        // Capture quark states
        foreach (var quark in quarks)
        {
            particles.Add(new ParticleSnapshot
            {
                ParticleId = quark.QuarkId,
                Type = quark.IsAntiParticle ? ParticleType.Antiquark : ParticleType.Quark,
                Position = quark.Position,
                Momentum = quark.Momentum,
                Energy = quark.Energy,
                Color = quark.Color,
                Charge = quark.ElectricCharge,
                Spin = quark.SpinZ,
                IsVirtual = false,
                BoundPartners = quark.BoundPartners
            });
        }
        
        // Capture interactions
        foreach (var interaction in interactions)
        {
            interactionSnapshots.Add(new InteractionSnapshot
            {
                InteractionId = interaction.InteractionId,
                Type = DetermineInteractionType(interaction),
                Particle1Id = interaction.Quark1Id,
                Particle2Id = interaction.Quark2Id,
                ForceVector = interaction.ForceVector,
                Strength = interaction.CouplingStrength,
                Range = GetInteractionRange(interaction)
            });
        }
        
        // Sample field points
        var fieldSamples = GenerateFieldSamplePoints(quarks);
        foreach (var point in fieldSamples)
        {
            var field = await CalculateFieldAtPoint(point, quarks);
            if (field.EnergyDensity > 1e-10) // Only store non-negligible fields
            {
                fields.Add(field);
            }
        }
        
        // Calculate metrics
        var metrics = CalculatePhysicsMetrics(quarks, stats);
        
        var snapshot = new SimulationSnapshot
        {
            Timestamp = DateTimeOffset.UtcNow,
            SimulationTime = stats.SimulationTime,
            Particles = particles,
            Interactions = interactionSnapshots,
            Fields = fields,
            Metrics = metrics
        };
        
        // Store snapshot
        _recentSnapshots.Add(snapshot);
        if (_recentSnapshots.Count > MaxSnapshots)
        {
            _recentSnapshots.RemoveAt(0);
        }
        
        return snapshot;
    }

    public Task<string> GenerateASCIIVisualization(SimulationSnapshot snapshot)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("╔═══════════════════════════════════════════════════════════════╗");
        sb.AppendLine($"║ Universe Simulation - Time: {snapshot.SimulationTime:F3}s       ║");
        sb.AppendLine("╠═══════════════════════════════════════════════════════════════╣");
        
        // Particle visualization (2D projection)
        var width = 60;
        var height = 20;
        var grid = new char[height, width];
        
        // Initialize grid
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                grid[y, x] = ' ';
            }
        }
        
        // Find bounds
        var minX = snapshot.Particles.Min(p => p.Position.X);
        var maxX = snapshot.Particles.Max(p => p.Position.X);
        var minY = snapshot.Particles.Min(p => p.Position.Y);
        var maxY = snapshot.Particles.Max(p => p.Position.Y);
        
        var rangeX = maxX - minX;
        var rangeY = maxY - minY;
        
        if (rangeX < 1) rangeX = 1;
        if (rangeY < 1) rangeY = 1;
        
        // Plot particles
        foreach (var particle in snapshot.Particles)
        {
            var x = (int)((particle.Position.X - minX) / rangeX * (width - 1));
            var y = (int)((particle.Position.Y - minY) / rangeY * (height - 1));
            
            if (x >= 0 && x < width && y >= 0 && y < height)
            {
                grid[height - 1 - y, x] = GetParticleChar(particle);
            }
        }
        
        // Draw grid
        sb.AppendLine("║ Particle Positions (2D Projection):                           ║");
        sb.AppendLine("║┌────────────────────────────────────────────────────────────┐║");
        
        for (int y = 0; y < height; y++)
        {
            sb.Append("║│");
            for (int x = 0; x < width; x++)
            {
                sb.Append(grid[y, x]);
            }
            sb.AppendLine("│║");
        }
        
        sb.AppendLine("║└────────────────────────────────────────────────────────────┘║");
        
        // Metrics
        sb.AppendLine("╠═══════════════════════════════════════════════════════════════╣");
        sb.AppendLine("║ Physics Metrics:                                              ║");
        sb.AppendLine($"║ Total Energy:     {snapshot.Metrics.TotalEnergy,8:F3} GeV                       ║");
        sb.AppendLine($"║ Total Momentum:   {snapshot.Metrics.TotalMomentum,8:F3} GeV/c                     ║");
        sb.AppendLine($"║ Free Quarks:      {snapshot.Metrics.FreeQuarks,8} (should be 0!)               ║");
        sb.AppendLine($"║ Color Singlets:   {snapshot.Metrics.ColorSinglets,8}                           ║");
        
        // Particle counts
        sb.AppendLine("║                                                               ║");
        sb.AppendLine("║ Particle Counts:                                              ║");
        foreach (var (type, count) in snapshot.Metrics.ParticleCounts)
        {
            sb.AppendLine($"║   {type,-15} {count,5}                                      ║");
        }
        
        sb.AppendLine("╠═══════════════════════════════════════════════════════════════╣");
        sb.AppendLine("║ Legend: u=up d=down c=charm s=strange t=top b=bottom         ║");
        sb.AppendLine("║         UPPERCASE=antiquark  *=gluon  @=bound state           ║");
        sb.AppendLine("╚═══════════════════════════════════════════════════════════════╝");
        
        return Task.FromResult(sb.ToString());
    }

    public Task<string> GenerateJsonVisualization(SimulationSnapshot snapshot)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        var json = JsonSerializer.Serialize(snapshot, options);
        return Task.FromResult(json);
    }

    public async Task SaveSnapshotToFile(SimulationSnapshot snapshot, string filePath)
    {
        var json = await GenerateJsonVisualization(snapshot);
        await File.WriteAllTextAsync(filePath, json);
        _logger.LogInformation("Saved snapshot to {FilePath}", filePath);
    }

    public Task<List<SimulationSnapshot>> GetRecentSnapshots(int count)
    {
        var snapshots = _recentSnapshots
            .TakeLast(count)
            .ToList();
        
        return Task.FromResult(snapshots);
    }

    private static InteractionType DetermineInteractionType(StrongForceInteraction interaction)
    {
        if (interaction.PotentialEnergy > 0)
            return InteractionType.Confinement;
        
        return InteractionType.Strong;
    }

    private static double GetInteractionRange(StrongForceInteraction interaction)
    {
        // Estimate effective range based on force strength
        var forceMagnitude = interaction.ForceVector.Length();
        if (forceMagnitude < 1e-10) return 1000.0; // Use large finite value instead of infinity
        
        return 1.0 / forceMagnitude;
    }

    private static List<System.Numerics.Vector3> GenerateFieldSamplePoints(List<QuarkState> quarks)
    {
        var points = new List<System.Numerics.Vector3>();
        
        if (!quarks.Any()) return points;
        
        // Sample around quark positions
        foreach (var quark in quarks.Take(10)) // Limit sampling for performance
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    for (int dz = -1; dz <= 1; dz++)
                    {
                        if (dx == 0 && dy == 0 && dz == 0) continue;
                        
                        var offset = new System.Numerics.Vector3(dx, dy, dz) * 0.1f;
                        points.Add(quark.Position + offset);
                    }
                }
            }
        }
        
        return points;
    }

    private async Task<FieldSnapshot> CalculateFieldAtPoint(System.Numerics.Vector3 point, 
        List<QuarkState> quarks)
    {
        var totalElectric = System.Numerics.Vector3.Zero;
        var totalMagnetic = System.Numerics.Vector3.Zero;
        var totalEnergy = 0.0;
        
        foreach (var quark in quarks)
        {
            var separation = point - quark.Position;
            var distance = separation.Length();
            
            if (distance < 1e-15f) continue;
            
            // Simplified field calculation
            var fieldStrength = QuantumChromodynamics.GetQCDPotential(distance);
            var direction = separation / distance;
            
            totalElectric += direction * (float)fieldStrength;
            totalEnergy += Math.Abs(fieldStrength);
        }
        
        return new FieldSnapshot
        {
            Position = point,
            ChromoelectricField = totalElectric,
            ChromomagneticField = totalMagnetic,
            FieldStrength = totalElectric.Length(),
            EnergyDensity = totalEnergy
        };
    }

    private static PhysicsMetrics CalculatePhysicsMetrics(List<QuarkState> quarks, 
        SimulationStats stats)
    {
        var particleCounts = new Dictionary<string, int>();
        
        // Count quarks by flavor
        foreach (var group in quarks.GroupBy(q => q.Flavor))
        {
            var name = group.Key.ToString();
            particleCounts[name] = group.Count(q => !q.IsAntiParticle);
            particleCounts["Anti" + name] = group.Count(q => q.IsAntiParticle);
        }
        
        particleCounts["Mesons"] = stats.Mesons;
        particleCounts["Baryons"] = stats.Baryons;
        
        return new PhysicsMetrics
        {
            TotalEnergy = stats.TotalEnergy,
            TotalMomentum = stats.TotalMomentum,
            TotalAngularMomentum = 0, // TODO: Calculate
            AverageBindingEnergy = 0, // TODO: Calculate
            VacuumEnergyDensity = QuantumFieldTheory.GetVacuumEnergyDensity(100),
            ColorSinglets = stats.Mesons + stats.Baryons,
            FreeQuarks = stats.FreeeQuarks,
            ParticleCounts = particleCounts
        };
    }

    private static char GetParticleChar(ParticleSnapshot particle)
    {
        if (particle.Type == ParticleType.Gluon) return '*';
        
        if (particle.BoundPartners.Any()) return '@';
        
        if (particle.Type == ParticleType.Quark)
        {
            // Determine flavor from mass (simplified)
            if (particle.Energy < 0.01) return 'u'; // Up or down
            if (particle.Energy < 0.2) return 'd';
            if (particle.Energy < 2) return 's';
            if (particle.Energy < 5) return 'c';
            if (particle.Energy < 10) return 'b';
            return 't';
        }
        
        if (particle.Type == ParticleType.Antiquark)
        {
            // Use uppercase for antiquarks
            if (particle.Energy < 0.01) return 'U';
            if (particle.Energy < 0.2) return 'D';
            if (particle.Energy < 2) return 'S';
            if (particle.Energy < 5) return 'C';
            if (particle.Energy < 10) return 'B';
            return 'T';
        }
        
        return '?';
    }
}