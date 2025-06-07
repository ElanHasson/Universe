using System.Numerics;

namespace Universe.Abstractions.Physics;

[GenerateSerializer]
public record QuarkState
{
    [Id(0)] public Guid QuarkId { get; init; }
    [Id(1)] public QuarkFlavor Flavor { get; init; }
    [Id(2)] public ColorCharge Color { get; init; }
    [Id(3)] public bool IsAntiParticle { get; init; }
    [Id(4)] public Vector3 Position { get; init; }
    [Id(5)] public Vector3 Momentum { get; init; }
    [Id(6)] public double SpinZ { get; init; } // +1/2 or -1/2
    [Id(7)] public double Energy { get; init; }
    [Id(8)] public DateTimeOffset LastUpdated { get; init; }
    [Id(9)] public List<Guid> BoundPartners { get; init; } = new();

    public double Mass => Flavor.GetMass();
    public double ElectricCharge => (IsAntiParticle ? -1 : 1) * Flavor.GetElectricCharge();
    
    public double GetKineticEnergy()
    {
        var p = Momentum.Length();
        var m = Mass;
        // Relativistic energy: E² = (pc)² + (mc²)²
        return Math.Sqrt(p * p + m * m) - m;
    }

    public Vector3 GetVelocity()
    {
        if (Energy <= 0) return Vector3.Zero;
        
        var gamma = Energy / Mass;
        return Momentum / (float)(Mass * gamma);
    }
}