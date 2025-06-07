using System.Numerics;

namespace Universe.Abstractions.Physics;

[GenerateSerializer]
public record ElectromagneticInteraction
{
    [Id(0)] public Guid InteractionId { get; init; }
    [Id(1)] public Guid Particle1Id { get; init; }
    [Id(2)] public Guid Particle2Id { get; init; }
    [Id(3)] public Vector3 ForceVector { get; init; }
    [Id(4)] public double PotentialEnergy { get; init; }
    [Id(5)] public double CouplingStrength { get; init; }
    [Id(6)] public bool IsVirtual { get; init; }
    [Id(7)] public DateTimeOffset Timestamp { get; init; }
}

[GenerateSerializer]
public record WeakInteraction
{
    [Id(0)] public Guid InteractionId { get; init; }
    [Id(1)] public Guid InitialParticleId { get; init; }
    [Id(2)] public Guid FinalParticleId { get; init; }
    [Id(3)] public WeakBosonType BosonType { get; init; }
    [Id(4)] public double InteractionStrength { get; init; }
    [Id(5)] public double DecayProbability { get; init; }
    [Id(6)] public DateTimeOffset Timestamp { get; init; }
}

public enum WeakBosonType
{
    WPlus,  // W⁺ boson
    WMinus, // W⁻ boson
    Z0      // Z⁰ boson
}

public static class ElectroweakTheory
{
    // Fundamental constants
    public const double FineStructureConstant = 1.0 / 137.035999206; // α
    public const double WeakMixingAngle = 0.23122; // sin²θw
    public const double FermiConstant = 1.1663787e-5; // GF in GeV⁻²
    
    // Boson masses
    public const double WBosonMass = 80.379; // GeV
    public const double ZBosonMass = 91.1876; // GeV
    public const double PhotonMass = 0.0; // Massless
    
    // Electromagnetic interaction
    public static ElectromagneticInteraction CalculateEMInteraction(QuarkState q1, QuarkState q2)
    {
        var separation = q2.Position - q1.Position;
        var distance = separation.Length();
        
        if (distance < 1e-15f) distance = 1e-15f;
        
        // Coulomb's law in natural units
        var charge1 = q1.ElectricCharge;
        var charge2 = q2.ElectricCharge;
        
        var potential = FineStructureConstant * charge1 * charge2 / distance;
        var forceMagnitude = FineStructureConstant * Math.Abs(charge1 * charge2) / (distance * distance);
        
        var forceDirection = separation / distance;
        
        // Opposite charges attract (force points inward)
        // Like charges repel (force points outward)
        // Force on particle 2 due to particle 1
        var forceVector = forceDirection * (float)forceMagnitude;
        if (charge1 * charge2 < 0) // Opposite charges
        {
            forceVector = -forceVector; // Attractive force points opposite to separation
        }
        
        return new ElectromagneticInteraction
        {
            InteractionId = Guid.NewGuid(),
            Particle1Id = q1.QuarkId,
            Particle2Id = q2.QuarkId,
            ForceVector = forceVector,
            PotentialEnergy = potential,
            CouplingStrength = FineStructureConstant,
            IsVirtual = distance < 1e-10, // Virtual photon for short-range
            Timestamp = DateTimeOffset.UtcNow
        };
    }
    
    // Weak interaction - beta decay
    public static WeakInteraction? CalculateWeakInteraction(QuarkState quark, double deltaTime)
    {
        // Only down-type quarks can undergo weak decay to up-type
        if (quark.Flavor is not (QuarkFlavor.Down or QuarkFlavor.Strange or QuarkFlavor.Bottom))
            return null;
        
        // CKM matrix elements (simplified)
        var decayProbability = GetWeakDecayProbability(quark.Flavor, deltaTime);
        
        if (Random.Shared.NextDouble() > decayProbability)
            return null;
        
        var finalFlavor = GetWeakDecayProduct(quark.Flavor);
        var bosonType = quark.IsAntiParticle ? WeakBosonType.WPlus : WeakBosonType.WMinus;
        
        return new WeakInteraction
        {
            InteractionId = Guid.NewGuid(),
            InitialParticleId = quark.QuarkId,
            FinalParticleId = Guid.NewGuid(), // Will be created by orchestrator
            BosonType = bosonType,
            InteractionStrength = FermiConstant,
            DecayProbability = decayProbability,
            Timestamp = DateTimeOffset.UtcNow
        };
    }
    
    private static double GetWeakDecayProbability(QuarkFlavor flavor, double deltaTime)
    {
        // Simplified decay rates
        var lifetime = flavor switch
        {
            QuarkFlavor.Down => 1e-10,    // Free down quark lifetime
            QuarkFlavor.Strange => 1e-8,   // Strange quark lifetime
            QuarkFlavor.Bottom => 1e-12,   // Bottom quark lifetime
            _ => double.PositiveInfinity
        };
        
        return 1 - Math.Exp(-deltaTime / lifetime);
    }
    
    private static QuarkFlavor GetWeakDecayProduct(QuarkFlavor initial)
    {
        return initial switch
        {
            QuarkFlavor.Down => QuarkFlavor.Up,
            QuarkFlavor.Strange => Random.Shared.NextDouble() < 0.95 ? QuarkFlavor.Up : QuarkFlavor.Charm,
            QuarkFlavor.Bottom => Random.Shared.NextDouble() < 0.9 ? QuarkFlavor.Charm : QuarkFlavor.Up,
            _ => initial
        };
    }
    
    // Weinberg angle relations
    public static double GetZCoupling(double charge, double isospin)
    {
        return isospin - 2 * charge * WeakMixingAngle;
    }
    
    // Running of electromagnetic coupling
    public static double GetRunningAlpha(double energyScale)
    {
        // α(μ) = α(0) / (1 - α(0)/(3π) * ln(μ²/m_e²))
        var electronMass = 0.000511; // GeV
        var correction = FineStructureConstant / (3 * Math.PI) * 
            Math.Log(energyScale * energyScale / (electronMass * electronMass));
        
        return FineStructureConstant / (1 - correction);
    }
    
    // Electroweak unification scale
    public static bool IsUnifiedRegime(double energyScale)
    {
        return energyScale > 100; // Above ~100 GeV, electromagnetic and weak merge
    }
    
    // GIM mechanism - suppresses flavor-changing neutral currents
    public static double GetGIMSuppression(QuarkFlavor flavor1, QuarkFlavor flavor2)
    {
        if (flavor1 == flavor2) return 1.0;
        
        // Different generations have suppressed transitions
        var gen1 = GetGeneration(flavor1);
        var gen2 = GetGeneration(flavor2);
        
        return Math.Pow(0.1, Math.Abs(gen1 - gen2));
    }
    
    private static int GetGeneration(QuarkFlavor flavor)
    {
        return flavor switch
        {
            QuarkFlavor.Up or QuarkFlavor.Down => 1,
            QuarkFlavor.Charm or QuarkFlavor.Strange => 2,
            QuarkFlavor.Top or QuarkFlavor.Bottom => 3,
            _ => 0
        };
    }
    
    // CP violation parameter
    public static Complex GetCPViolationPhase()
    {
        // Jarlskog invariant ~ 3×10⁻⁵
        var phase = 1.2; // radians
        return new Complex(Math.Cos(phase), Math.Sin(phase));
    }
}