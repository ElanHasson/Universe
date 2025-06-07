using System.Numerics;
using Universe.Abstractions.Physics;
using Xunit;

namespace Universe.Tests.Physics;

public class ElectroweakTests
{
    [Fact]
    public void ElectromagneticInteraction_ShouldFollowCoulombsLaw()
    {
        var quark1 = new QuarkState
        {
            QuarkId = Guid.NewGuid(),
            Flavor = QuarkFlavor.Up, // +2/3 charge
            Color = ColorCharge.Red,
            Position = Vector3.Zero,
            Momentum = Vector3.Zero,
            Energy = QuarkFlavor.Up.GetMass()
        };
        
        var quark2 = new QuarkState
        {
            QuarkId = Guid.NewGuid(),
            Flavor = QuarkFlavor.Down, // -1/3 charge
            Color = ColorCharge.Green,
            Position = new Vector3(1, 0, 0),
            Momentum = Vector3.Zero,
            Energy = QuarkFlavor.Down.GetMass()
        };
        
        var interaction = ElectroweakTheory.CalculateEMInteraction(quark1, quark2);
        
        // Opposite charges should attract
        Assert.True(interaction.ForceVector.X < 0); // Force on q2 towards q1
        
        // Check 1/r² dependence
        var quark3 = quark2 with { Position = new Vector3(2, 0, 0) };
        var interaction2 = ElectroweakTheory.CalculateEMInteraction(quark1, quark3);
        
        var forceRatio = interaction.ForceVector.Length() / interaction2.ForceVector.Length();
        Assert.Equal(4.0, forceRatio, 1); // F ∝ 1/r²
    }
    
    [Fact]
    public void FineStructureConstant_ShouldBeCorrect()
    {
        Assert.Equal(1.0 / 137.035999206, ElectroweakTheory.FineStructureConstant, 10);
    }
    
    [Fact]
    public void WeakMixingAngle_ShouldBeInRange()
    {
        Assert.InRange(ElectroweakTheory.WeakMixingAngle, 0.2, 0.25);
    }
    
    [Fact]
    public void RunningAlpha_ShouldIncreaseWithEnergy()
    {
        var lowEnergy = 0.001; // 1 MeV
        var highEnergy = 100.0; // 100 GeV
        
        var alphaLow = ElectroweakTheory.GetRunningAlpha(lowEnergy);
        var alphaHigh = ElectroweakTheory.GetRunningAlpha(highEnergy);
        
        // EM coupling increases with energy (opposite to strong force)
        Assert.True(alphaHigh > alphaLow);
    }
    
    [Fact]
    public void ElectroweakUnification_ShouldOccurAtHighEnergy()
    {
        Assert.False(ElectroweakTheory.IsUnifiedRegime(10)); // 10 GeV
        Assert.True(ElectroweakTheory.IsUnifiedRegime(200)); // 200 GeV
    }
    
    [Fact]
    public void WeakDecay_ShouldOnlyAffectDownTypeQuarks()
    {
        var upQuark = new QuarkState { Flavor = QuarkFlavor.Up };
        var downQuark = new QuarkState { Flavor = QuarkFlavor.Down };
        
        var upDecay = ElectroweakTheory.CalculateWeakInteraction(upQuark, 1e-10);
        var downDecay = ElectroweakTheory.CalculateWeakInteraction(downQuark, 1e-10);
        
        Assert.Null(upDecay); // Up quarks don't decay via weak force
        // Down quarks might decay (probabilistic)
    }
    
    [Fact]
    public void GIMSuppression_ShouldSuppressFlavorChanging()
    {
        // Same generation - no suppression
        var sameGen = ElectroweakTheory.GetGIMSuppression(QuarkFlavor.Up, QuarkFlavor.Down);
        Assert.Equal(1.0, sameGen);
        
        // Different generations - suppressed
        var diffGen1 = ElectroweakTheory.GetGIMSuppression(QuarkFlavor.Up, QuarkFlavor.Strange);
        var diffGen2 = ElectroweakTheory.GetGIMSuppression(QuarkFlavor.Up, QuarkFlavor.Bottom);
        
        Assert.True(diffGen1 < 1.0);
        Assert.True(diffGen2 < diffGen1); // More suppressed for larger generation difference
    }
    
    [Fact]
    public void BosonMasses_ShouldBeCorrect()
    {
        Assert.Equal(80.379, ElectroweakTheory.WBosonMass, 3);
        Assert.Equal(91.1876, ElectroweakTheory.ZBosonMass, 4);
        Assert.Equal(0.0, ElectroweakTheory.PhotonMass);
    }
    
    [Fact]
    public void ZCoupling_ShouldDependOnWeakIsospin()
    {
        // Left-handed up quark: I3 = +1/2, Q = +2/3
        var upLeft = ElectroweakTheory.GetZCoupling(2.0/3.0, 0.5);
        
        // Left-handed down quark: I3 = -1/2, Q = -1/3
        var downLeft = ElectroweakTheory.GetZCoupling(-1.0/3.0, -0.5);
        
        Assert.NotEqual(upLeft, downLeft);
    }
    
    [Fact]
    public void CPViolationPhase_ShouldBeComplex()
    {
        var phase = ElectroweakTheory.GetCPViolationPhase();
        
        // Should have both real and imaginary parts
        Assert.NotEqual(0, phase.Real);
        Assert.NotEqual(0, phase.Imaginary);
        
        // Magnitude should be 1 (unitary)
        Assert.Equal(1.0, Complex.Abs(phase), 10);
    }
    
    [Theory]
    [InlineData(QuarkFlavor.Down, 1e-10)]
    [InlineData(QuarkFlavor.Strange, 1e-8)]
    [InlineData(QuarkFlavor.Bottom, 1e-12)]
    public void WeakDecayProbability_ShouldMatchLifetimes(QuarkFlavor flavor, double expectedLifetime)
    {
        var quark = new QuarkState { Flavor = flavor };
        var deltaTime = expectedLifetime / 10;
        
        // Run many trials to get average decay probability
        var decayCount = 0;
        for (int i = 0; i < 1000; i++)
        {
            var interaction = ElectroweakTheory.CalculateWeakInteraction(quark, deltaTime);
            if (interaction != null) decayCount++;
        }
        
        var measuredProbability = decayCount / 1000.0;
        var expectedProbability = 1 - Math.Exp(-deltaTime / expectedLifetime);
        
        Assert.Equal(expectedProbability, measuredProbability, 1);
    }
}