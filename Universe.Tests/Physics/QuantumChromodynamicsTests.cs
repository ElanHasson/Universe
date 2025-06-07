using System.Numerics;
using Universe.Abstractions.Physics;
using Xunit;

namespace Universe.Tests.Physics;

public class QuantumChromodynamicsTests
{
    [Fact]
    public void RunningCoupling_ShouldDecreaseAtHighEnergy()
    {
        // Asymptotic freedom test
        var lowEnergy = 1.0; // 1 GeV
        var highEnergy = 100.0; // 100 GeV
        
        var alphaLow = QuantumChromodynamics.GetRunningCoupling(lowEnergy);
        var alphaHigh = QuantumChromodynamics.GetRunningCoupling(highEnergy);
        
        Assert.True(alphaHigh < alphaLow, 
            "Coupling should decrease at higher energies (asymptotic freedom)");
    }
    
    [Fact]
    public void RunningCoupling_ShouldBeLargeAtLowEnergy()
    {
        // Confinement regime
        var veryLowEnergy = 0.1; // 100 MeV
        var alpha = QuantumChromodynamics.GetRunningCoupling(veryLowEnergy);
        
        Assert.True(alpha > 0.5, 
            "Coupling should be large in confinement regime");
    }
    
    [Theory]
    [InlineData(0.1, true)]  // Short distance - perturbative
    [InlineData(1.0, false)] // Long distance - non-perturbative
    public void PerturbativeRegime_ShouldDependOnDistance(double distance, bool expectedPerturbative)
    {
        var isPerturbative = QuantumChromodynamics.IsPerturbativeRegime(distance);
        Assert.Equal(expectedPerturbative, isPerturbative);
    }
    
    [Fact]
    public void QCDPotential_ShouldShowCornellBehavior()
    {
        // V(r) = -4/3 * αs/r + kr
        var shortDistance = 0.01; // fm
        var mediumDistance = 0.5;  // fm
        var longDistance = 2.0;    // fm
        
        var vShort = QuantumChromodynamics.GetQCDPotential(shortDistance);
        var vMedium = QuantumChromodynamics.GetQCDPotential(mediumDistance);
        var vLong = QuantumChromodynamics.GetQCDPotential(longDistance);
        
        // At short distances, potential should be negative (attractive)
        Assert.True(vShort < 0);
        
        // At long distances, linear term dominates (confinement)
        Assert.True(vLong > vMedium);
        
        // Check linear growth at large distances
        var v2Long = QuantumChromodynamics.GetQCDPotential(2 * longDistance);
        var linearGrowth = (v2Long - vLong) / vLong;
        Assert.True(linearGrowth > 0.8, "Potential should grow approximately linearly at large distances");
    }
    
    [Fact]
    public void ColorFactor_ShouldBeCorrect()
    {
        // Same color quarks repel
        var sameFactor = QuantumChromodynamics.GetColorFactor(ColorCharge.Red, ColorCharge.Red);
        Assert.True(sameFactor < 0);
        
        // Color-anticolor attract strongly
        var oppositeFactor = QuantumChromodynamics.GetColorFactor(ColorCharge.Red, ColorCharge.AntiRed);
        Assert.True(oppositeFactor > 0);
        Assert.True(Math.Abs(oppositeFactor) > Math.Abs(sameFactor));
        
        // Different colors have intermediate attraction
        var differentFactor = QuantumChromodynamics.GetColorFactor(ColorCharge.Red, ColorCharge.Green);
        Assert.True(differentFactor < 0);
        Assert.True(Math.Abs(differentFactor) < Math.Abs(oppositeFactor));
    }
    
    [Fact]
    public void BetaFunction_ShouldBeNegative()
    {
        // QCD beta function is negative (asymptotic freedom)
        var alpha = 0.1;
        var beta = QuantumChromodynamics.BetaFunction(alpha);
        
        Assert.True(beta < 0, "QCD beta function should be negative");
    }
    
    [Fact]
    public void GluonExchange_ShouldConserveColor()
    {
        // Test color conservation in gluon exchange
        var allowed = QuantumChromodynamics.IsGluonExchangeAllowed(
            ColorCharge.Red, ColorCharge.Green,
            ColorCharge.Green, ColorCharge.Red);
        
        Assert.True(allowed);
        
        var notAllowed = QuantumChromodynamics.IsGluonExchangeAllowed(
            ColorCharge.Red, ColorCharge.Green,
            ColorCharge.Blue, ColorCharge.Red);
        
        Assert.False(notAllowed);
    }
    
    [Fact]
    public void StrongForce_ShouldBeAttractiveForColorAnticolor()
    {
        var quark1 = new QuarkState
        {
            QuarkId = Guid.NewGuid(),
            Flavor = QuarkFlavor.Up,
            Color = ColorCharge.Red,
            Position = Vector3.Zero,
            Momentum = Vector3.Zero,
            Energy = QuarkFlavor.Up.GetMass()
        };
        
        var quark2 = new QuarkState
        {
            QuarkId = Guid.NewGuid(),
            Flavor = QuarkFlavor.Down,
            Color = ColorCharge.AntiRed,
            IsAntiParticle = true,
            Position = new Vector3(0.5f, 0, 0),
            Momentum = Vector3.Zero,
            Energy = QuarkFlavor.Down.GetMass()
        };
        
        var force = QuantumChromodynamics.CalculateStrongForce(quark1, quark2);
        
        // Force should point from q1 to q2 (attractive)
        Assert.True(force.X > 0);
    }
    
    [Theory]
    [InlineData(0.01)]  // Very short distance
    [InlineData(1.0)]   // Medium distance
    [InlineData(10.0)]  // Large distance
    public void FluxTubeEnergy_ShouldScaleWithDistance(double distance)
    {
        var energy = QuantumChromodynamics.GetFluxTubeEnergy(distance);
        // Flux tube energy = string tension * distance
        var expectedEnergy = QuantumChromodynamics.StringTension * distance;
        Assert.Equal(expectedEnergy, energy, 6);
    }
}