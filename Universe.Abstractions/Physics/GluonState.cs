using System.Numerics;

namespace Universe.Abstractions.Physics;

[GenerateSerializer]
public record GluonState
{
    [Id(0)] public Guid GluonId { get; init; }
    [Id(1)] public ColorCharge ColorCharge1 { get; init; } // Color
    [Id(2)] public ColorCharge ColorCharge2 { get; init; } // Anti-color
    [Id(3)] public Vector3 Position { get; init; }
    [Id(4)] public Vector3 Momentum { get; init; }
    [Id(5)] public double Energy { get; init; }
    [Id(6)] public double Polarization { get; init; } // Gluon polarization state
    [Id(7)] public Guid SourceQuarkId { get; init; }
    [Id(8)] public Guid TargetQuarkId { get; init; }
    [Id(9)] public DateTimeOffset CreatedAt { get; init; }
    [Id(10)] public double PropagationTime { get; init; }
    
    public bool IsVirtual => PropagationTime < 1e-23; // Virtual gluons exist for very short times
    
    public double GetFieldStrength(Vector3 point)
    {
        var distance = (point - Position).Length();
        if (distance < 1e-15f) return 0;
        
        // Gluon field strength decreases with distance but doesn't vanish (confinement)
        var alphaS = 0.1185; // Strong coupling constant
        return alphaS / (distance * distance) * Math.Exp(-distance / 1.0); // 1 fm screening length
    }
}

[GenerateSerializer]
public record GluonFieldTensor
{
    [Id(0)] public Vector3 ElectricField { get; init; } // Chromoelectric field
    [Id(1)] public Vector3 MagneticField { get; init; } // Chromomagnetic field
    [Id(2)] public double[,] ColorMatrix { get; init; } = new double[8, 8]; // SU(3) gauge field
    
    public double GetEnergyDensity()
    {
        var e2 = ElectricField.LengthSquared();
        var b2 = MagneticField.LengthSquared();
        return 0.5 * (e2 + b2); // Simplified QCD energy density
    }
}