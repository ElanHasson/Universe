using System.Numerics;
using Universe.Abstractions.Physics;
using Xunit;

namespace Universe.Tests.Physics;

public class QuarkPhysicsTests
{
    [Theory]
    [InlineData(QuarkFlavor.Up, 2.2e-3)]
    [InlineData(QuarkFlavor.Down, 4.7e-3)]
    [InlineData(QuarkFlavor.Charm, 1.28)]
    [InlineData(QuarkFlavor.Strange, 0.096)]
    [InlineData(QuarkFlavor.Top, 173.1)]
    [InlineData(QuarkFlavor.Bottom, 4.18)]
    public void QuarkMass_ShouldMatchStandardModel(QuarkFlavor flavor, double expectedMass)
    {
        var actualMass = flavor.GetMass();
        Assert.Equal(expectedMass, actualMass, 3);
    }

    [Theory]
    [InlineData(QuarkFlavor.Up, 2.0 / 3.0)]
    [InlineData(QuarkFlavor.Charm, 2.0 / 3.0)]
    [InlineData(QuarkFlavor.Top, 2.0 / 3.0)]
    [InlineData(QuarkFlavor.Down, -1.0 / 3.0)]
    [InlineData(QuarkFlavor.Strange, -1.0 / 3.0)]
    [InlineData(QuarkFlavor.Bottom, -1.0 / 3.0)]
    public void QuarkCharge_ShouldMatchStandardModel(QuarkFlavor flavor, double expectedCharge)
    {
        var actualCharge = flavor.GetElectricCharge();
        Assert.Equal(expectedCharge, actualCharge, 10);
    }

    [Fact]
    public void ColorNeutrality_MesonConfiguration_ShouldBeNeutral()
    {
        // Meson: color + anticolor
        Assert.True(ColorChargeExtensions.IsColorNeutral(ColorCharge.Red, ColorCharge.AntiRed));
        Assert.True(ColorChargeExtensions.IsColorNeutral(ColorCharge.Green, ColorCharge.AntiGreen));
        Assert.True(ColorChargeExtensions.IsColorNeutral(ColorCharge.Blue, ColorCharge.AntiBlue));
    }

    [Fact]
    public void ColorNeutrality_BaryonConfiguration_ShouldBeNeutral()
    {
        // Baryon: red + green + blue
        Assert.True(ColorChargeExtensions.IsColorNeutral(
            ColorCharge.Red, ColorCharge.Green, ColorCharge.Blue));
        
        // Antibaryon: antired + antigreen + antiblue
        Assert.True(ColorChargeExtensions.IsColorNeutral(
            ColorCharge.AntiRed, ColorCharge.AntiGreen, ColorCharge.AntiBlue));
    }

    [Fact]
    public void ColorNeutrality_InvalidConfiguration_ShouldNotBeNeutral()
    {
        // Two same colors
        Assert.False(ColorChargeExtensions.IsColorNeutral(ColorCharge.Red, ColorCharge.Red));
        
        // Three same colors
        Assert.False(ColorChargeExtensions.IsColorNeutral(
            ColorCharge.Red, ColorCharge.Red, ColorCharge.Red));
        
        // Mixed regular and anti colors in baryon
        Assert.False(ColorChargeExtensions.IsColorNeutral(
            ColorCharge.Red, ColorCharge.Green, ColorCharge.AntiBlue));
    }

    [Fact]
    public void StrongForce_ShortDistance_ShouldBeCoulombLike()
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
            Color = ColorCharge.Green,
            Position = new Vector3(1e-15f, 0, 0), // Very short distance
            Momentum = Vector3.Zero,
            Energy = QuarkFlavor.Down.GetMass()
        };

        var interaction = StrongForceCalculator.CalculateInteraction(quark1, quark2);
        
        // At short distances, the force should be attractive (negative potential)
        Assert.True(interaction.PotentialEnergy < 0);
    }

    [Fact]
    public void StrongForce_LongDistance_ShouldShowConfinement()
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
            Color = ColorCharge.Green,
            Position = new Vector3(1f, 0, 0), // Long distance (1 fm)
            Momentum = Vector3.Zero,
            Energy = QuarkFlavor.Down.GetMass()
        };

        var interaction = StrongForceCalculator.CalculateInteraction(quark1, quark2);
        
        // At long distances, confinement should dominate (positive force magnitude)
        Assert.True(interaction.PotentialEnergy > 0);
    }

    [Fact]
    public void QuarkState_RelativisticEnergy_ShouldBeCalculatedCorrectly()
    {
        var mass = QuarkFlavor.Up.GetMass();
        var momentum = new Vector3(1, 0, 0); // 1 GeV/c momentum
        var energy = Math.Sqrt(1 + mass * mass); // E² = p² + m²
        
        var quark = new QuarkState
        {
            QuarkId = Guid.NewGuid(),
            Flavor = QuarkFlavor.Up,
            Color = ColorCharge.Red,
            Position = Vector3.Zero,
            Momentum = momentum,
            Energy = energy
        };

        var kineticEnergy = quark.GetKineticEnergy();
        var expectedKE = energy - mass;
        
        Assert.Equal(expectedKE, kineticEnergy, 6);
    }

    [Fact]
    public void HadronFormation_ShouldConserveQuantumNumbers()
    {
        // Test baryon number conservation
        var protonQuarks = new[] { QuarkFlavor.Up, QuarkFlavor.Up, QuarkFlavor.Down };
        var totalCharge = protonQuarks.Sum(f => f.GetElectricCharge());
        Assert.Equal(1.0, totalCharge, 10); // Proton has charge +1
        
        // Test meson quantum numbers
        var pionQuarks = new[] { QuarkFlavor.Up, QuarkFlavor.Down };
        var pionCharge = pionQuarks[0].GetElectricCharge() - pionQuarks[1].GetElectricCharge(); // Anti-down
        Assert.Equal(1.0, pionCharge, 10); // π+ has charge +1
    }

    [Theory]
    [InlineData(ColorCharge.Red, ColorCharge.Green, ColorCharge.Blue, true)] // RGB
    [InlineData(ColorCharge.Red, ColorCharge.Red, ColorCharge.Blue, false)]  // RRB - invalid
    [InlineData(ColorCharge.Red, ColorCharge.AntiRed, null, true)]          // R-antiR meson
    public void ColorConfinement_ShouldEnforceColorNeutrality(
        ColorCharge c1, ColorCharge c2, ColorCharge? c3, bool shouldBeNeutral)
    {
        if (c3.HasValue)
        {
            // Three-quark system (baryon)
            var isNeutral = ColorChargeExtensions.IsColorNeutral(c1, c2, c3.Value);
            Assert.Equal(shouldBeNeutral, isNeutral);
        }
        else
        {
            // Two-quark system (meson)
            var isNeutral = ColorChargeExtensions.IsColorNeutral(c1, c2);
            Assert.Equal(shouldBeNeutral, isNeutral);
        }
    }
}