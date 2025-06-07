using Universe.Abstractions.Physics;
using System.Numerics;

namespace Universe.Abstractions.Grains;

public interface IHadronGrain : IGrainWithGuidKey
{
    Task<HadronState> GetState();
    Task Initialize(List<Guid> constituentQuarkIds, HadronType type);
    Task<Vector3> GetCenterOfMass();
    Task<double> GetInvariantMass();
    Task<bool> IsStable();
    Task<List<QuarkState>> GetConstituentQuarks();
    Task Decay(); // For unstable hadrons
    Task Evolve(double deltaTime);
}

public enum HadronType
{
    Meson,      // Quark-antiquark pair
    Baryon,     // Three quarks
    AntiBaryon  // Three antiquarks
}

[GenerateSerializer]
public record HadronState
{
    [Id(0)] public Guid HadronId { get; init; }
    [Id(1)] public HadronType Type { get; init; }
    [Id(2)] public List<Guid> ConstituentQuarkIds { get; init; } = new();
    [Id(3)] public Vector3 Position { get; init; }
    [Id(4)] public Vector3 Momentum { get; init; }
    [Id(5)] public double Mass { get; init; }
    [Id(6)] public double BindingEnergy { get; init; }
    [Id(7)] public bool IsStable { get; init; }
    [Id(8)] public double Lifetime { get; init; } // For unstable particles
    [Id(9)] public DateTimeOffset CreatedAt { get; init; }
}