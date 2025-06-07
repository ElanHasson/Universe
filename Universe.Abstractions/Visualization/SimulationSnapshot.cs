using System.Numerics;
using Universe.Abstractions.Physics;

namespace Universe.Abstractions.Visualization;

[GenerateSerializer]
public record SimulationSnapshot
{
    [Id(0)] public DateTimeOffset Timestamp { get; init; }
    [Id(1)] public double SimulationTime { get; init; }
    [Id(2)] public List<ParticleSnapshot> Particles { get; init; } = new();
    [Id(3)] public List<InteractionSnapshot> Interactions { get; init; } = new();
    [Id(4)] public List<FieldSnapshot> Fields { get; init; } = new();
    [Id(5)] public PhysicsMetrics Metrics { get; init; } = new();
}

[GenerateSerializer]
public record ParticleSnapshot
{
    [Id(0)] public Guid ParticleId { get; init; }
    [Id(1)] public ParticleType Type { get; init; }
    [Id(2)] public Vector3 Position { get; init; }
    [Id(3)] public Vector3 Momentum { get; init; }
    [Id(4)] public double Energy { get; init; }
    [Id(5)] public ColorCharge? Color { get; init; }
    [Id(6)] public double Charge { get; init; }
    [Id(7)] public double Spin { get; init; }
    [Id(8)] public bool IsVirtual { get; init; }
    [Id(9)] public List<Guid> BoundPartners { get; init; } = new();
}

[GenerateSerializer]
public record InteractionSnapshot
{
    [Id(0)] public Guid InteractionId { get; init; }
    [Id(1)] public InteractionType Type { get; init; }
    [Id(2)] public Guid Particle1Id { get; init; }
    [Id(3)] public Guid Particle2Id { get; init; }
    [Id(4)] public Vector3 ForceVector { get; init; }
    [Id(5)] public double Strength { get; init; }
    [Id(6)] public double Range { get; init; }
}

[GenerateSerializer]
public record FieldSnapshot
{
    [Id(0)] public Vector3 Position { get; init; }
    [Id(1)] public Vector3 ChromoelectricField { get; init; }
    [Id(2)] public Vector3 ChromomagneticField { get; init; }
    [Id(3)] public double FieldStrength { get; init; }
    [Id(4)] public double EnergyDensity { get; init; }
}

[GenerateSerializer]
public record PhysicsMetrics
{
    [Id(0)] public double TotalEnergy { get; init; }
    [Id(1)] public double TotalMomentum { get; init; }
    [Id(2)] public double TotalAngularMomentum { get; init; }
    [Id(3)] public double AverageBindingEnergy { get; init; }
    [Id(4)] public double VacuumEnergyDensity { get; init; }
    [Id(5)] public int ColorSinglets { get; init; }
    [Id(6)] public int FreeQuarks { get; init; }
    [Id(7)] public Dictionary<string, int> ParticleCounts { get; init; } = new();
}

public enum ParticleType
{
    Quark,
    Antiquark,
    Gluon,
    Photon,
    WBoson,
    ZBoson,
    Meson,
    Baryon
}

public enum InteractionType
{
    Strong,
    Electromagnetic,
    Weak,
    Confinement
}