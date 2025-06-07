using Universe.Abstractions.Physics;
using System.Numerics;

namespace Universe.Abstractions.Grains;

public interface ISimulationOrchestratorGrain : IGrainWithIntegerKey
{
    Task<Guid> CreateQuark(QuarkFlavor flavor, ColorCharge color, bool isAntiParticle, Vector3 position, Vector3 momentum, double spinZ);
    Task<List<Guid>> CreateQuarkPair(QuarkFlavor flavor, Vector3 position); // Creates quark-antiquark pair
    Task<List<Guid>> CreateMeson(QuarkFlavor quark1Flavor, QuarkFlavor quark2Flavor, Vector3 position);
    Task<List<Guid>> CreateBaryon(QuarkFlavor[] flavors, Vector3 position);
    Task<List<QuarkState>> GetAllQuarks();
    Task<List<StrongForceInteraction>> GetActiveInteractions();
    Task RunSimulationStep(double deltaTime);
    Task<SimulationStats> GetSimulationStats();
    Task Reset();
    Task Pause();
    Task Resume();
}

[GenerateSerializer]
public record SimulationStats
{
    [Id(0)] public int TotalQuarks { get; init; }
    [Id(1)] public int FreeeQuarks { get; init; }
    [Id(2)] public int ConfinedQuarks { get; init; }
    [Id(3)] public int Mesons { get; init; }
    [Id(4)] public int Baryons { get; init; }
    [Id(5)] public double TotalEnergy { get; init; }
    [Id(6)] public double TotalMomentum { get; init; }
    [Id(7)] public double SimulationTime { get; init; }
    [Id(8)] public int InteractionCount { get; init; }
}