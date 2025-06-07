using System.Numerics;

namespace Universe.Abstractions.Physics;

[GenerateSerializer]
public record StrongForceInteraction
{
    [Id(0)] public Guid InteractionId { get; init; }
    [Id(1)] public Guid Quark1Id { get; init; }
    [Id(2)] public Guid Quark2Id { get; init; }
    [Id(3)] public ColorCharge GluonColor1 { get; init; } // Color carried away from quark1
    [Id(4)] public ColorCharge GluonColor2 { get; init; } // Color carried to quark2
    [Id(5)] public double CouplingStrength { get; init; }
    [Id(6)] public Vector3 ForceVector { get; init; }
    [Id(7)] public double PotentialEnergy { get; init; }
    [Id(8)] public DateTimeOffset Timestamp { get; init; }
    [Id(9)] public bool IsPerturbative { get; init; }
    [Id(10)] public double ColorFactor { get; init; }
    [Id(11)] public Guid? GluonId { get; init; } // ID of exchanged gluon
}

public static class StrongForceCalculator
{
    public static StrongForceInteraction CalculateInteraction(QuarkState quark1, QuarkState quark2)
    {
        var separation = quark2.Position - quark1.Position;
        var distance = separation.Length();
        
        if (distance < 1e-15f) // Avoid singularity
            distance = 1e-15f;

        // Use proper QCD calculations
        var forceVector = QuantumChromodynamics.CalculateStrongForce(quark1, quark2);
        var potential = QuantumChromodynamics.GetQCDPotential(distance);
        var alphaS = QuantumChromodynamics.GetRunningCoupling(1.0 / distance);
        var colorFactor = QuantumChromodynamics.GetColorFactor(quark1.Color, quark2.Color);
        var isPerturbative = QuantumChromodynamics.IsPerturbativeRegime(distance);

        // Determine gluon exchange colors
        var (gluonColor1, gluonColor2) = DetermineGluonColors(quark1.Color, quark2.Color);

        return new StrongForceInteraction
        {
            InteractionId = Guid.NewGuid(),
            Quark1Id = quark1.QuarkId,
            Quark2Id = quark2.QuarkId,
            GluonColor1 = gluonColor1,
            GluonColor2 = gluonColor2,
            CouplingStrength = alphaS,
            ForceVector = forceVector,
            PotentialEnergy = potential,
            Timestamp = DateTimeOffset.UtcNow,
            IsPerturbative = isPerturbative,
            ColorFactor = colorFactor,
            GluonId = null // Will be set if actual gluon is created
        };
    }

    private static (ColorCharge, ColorCharge) DetermineGluonColors(ColorCharge color1, ColorCharge color2)
    {
        // Gluons carry color-anticolor pairs
        // For quark-quark interaction, we need to determine which colors are exchanged
        
        // If quarks have the same color, they can't exchange a gluon directly
        if (color1 == color2)
        {
            // Need to go through intermediate color
            var intermediateColors = Enum.GetValues<ColorCharge>()
                .Where(c => c != color1 && !c.IsAntiColor())
                .ToArray();
            
            if (intermediateColors.Length > 0)
            {
                var intermediate = intermediateColors[Random.Shared.Next(intermediateColors.Length)];
                return (intermediate, intermediate.GetAntiColor());
            }
        }
        
        // For different colors, gluon carries appropriate color-anticolor pair
        return (color2, color1.GetAntiColor());
    }
    
    public static bool ShouldExchangeGluon(QuarkState quark1, QuarkState quark2, double deltaTime)
    {
        var separation = (quark2.Position - quark1.Position).Length();
        var alphaS = QuantumChromodynamics.GetRunningCoupling(1.0 / separation);
        
        // Probability of gluon exchange proportional to coupling strength and time
        var exchangeProbability = alphaS * deltaTime * 100; // Simplified probability
        
        return Random.Shared.NextDouble() < exchangeProbability;
    }
}