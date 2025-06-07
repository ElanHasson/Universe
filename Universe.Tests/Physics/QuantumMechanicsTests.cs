using System.Numerics;
using Universe.Abstractions.Physics;
using Xunit;

namespace Universe.Tests.Physics;

public class QuantumMechanicsTests
{
    [Fact]
    public void SpinState_ExpectationValue_ShouldBeCorrect()
    {
        // Test spin up state
        var spinUp = SpinState.CreateSpinUp();
        Assert.Equal(0.5, spinUp.GetSpinExpectation());
        
        // Test spin down state
        var spinDown = SpinState.CreateSpinDown();
        Assert.Equal(-0.5, spinDown.GetSpinExpectation());
        
        // Test superposition state
        var superposition = new SpinState
        {
            SpinUp = new Complex(1 / Math.Sqrt(2), 0),
            SpinDown = new Complex(1 / Math.Sqrt(2), 0)
        };
        Assert.Equal(0, superposition.GetSpinExpectation(), 3);
    }
    
    [Fact]
    public void HeisenbergUncertainty_ShouldHold()
    {
        var positionUncertainty = 1e-15; // 1 fm
        var momentumUncertainty = QuantumMechanics.GetMinimumUncertainty(positionUncertainty);
        
        // ΔxΔp ≥ ℏ/2 (with ℏ=1 in natural units)
        var product = positionUncertainty * momentumUncertainty;
        Assert.True(product >= 0.5);
    }
    
    [Fact]
    public void WaveFunction_ProbabilityDensity_ShouldBeNormalized()
    {
        var positions = new[]
        {
            Vector3.Zero,
            new Vector3(1, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(0, 0, 1)
        };
        
        var amplitudes = new[]
        {
            new Complex(0.5, 0),
            new Complex(0.5, 0),
            new Complex(0.5, 0),
            new Complex(0.5, 0)
        };
        
        var waveFunction = new WaveFunction
        {
            Positions = positions,
            Amplitudes = amplitudes,
            Normalization = 1.0
        };
        
        var totalProbability = 0.0;
        foreach (var pos in positions)
        {
            totalProbability += waveFunction.GetProbabilityDensity(pos);
        }
        
        Assert.Equal(1.0, totalProbability, 3);
    }
    
    [Fact]
    public void TimeEvolution_ShouldPreserveNormalization()
    {
        var waveFunction = new WaveFunction
        {
            Positions = new[] { Vector3.Zero },
            Amplitudes = new[] { Complex.One },
            Normalization = 1.0
        };
        
        var hamiltonian = 2.0; // Energy in natural units
        var time = 1.0;
        
        var evolved = QuantumMechanics.TimeEvolve(waveFunction, hamiltonian, time);
        
        var probability = (evolved.Amplitudes[0] * Complex.Conjugate(evolved.Amplitudes[0])).Real;
        Assert.Equal(1.0, probability, 10);
    }
    
    [Fact]
    public void QuantumMeasurement_ShouldCollapseWaveFunction()
    {
        var waveFunction = new WaveFunction
        {
            Positions = new[]
            {
                new Vector3(0, 0, 0),
                new Vector3(1, 0, 0),
                new Vector3(2, 0, 0)
            },
            Amplitudes = new[]
            {
                new Complex(1 / Math.Sqrt(3), 0),
                new Complex(1 / Math.Sqrt(3), 0),
                new Complex(1 / Math.Sqrt(3), 0)
            },
            Normalization = 1.0
        };
        
        var (measuredPosition, collapsedWF) = QuantumMechanics.MeasurePosition(waveFunction);
        
        // After measurement, only one position should have non-zero amplitude
        var nonZeroCount = collapsedWF.Amplitudes.Count(a => Complex.Abs(a) > 0);
        Assert.Equal(1, nonZeroCount);
        
        // Measured position should be one of the original positions
        Assert.Contains(measuredPosition, waveFunction.Positions);
    }
    
    [Fact]
    public void SpinMeasurement_ShouldGiveCorrectProbabilities()
    {
        var spinUp = SpinState.CreateSpinUp();
        var spinDown = SpinState.CreateSpinDown();
        
        // Measure spin up state many times
        var upResults = 0;
        for (int i = 0; i < 1000; i++)
        {
            var (spin, _) = QuantumMechanics.MeasureSpin(spinUp);
            if (spin > 0) upResults++;
        }
        
        // Should always measure spin up
        Assert.Equal(1000, upResults);
        
        // Measure superposition state
        var superposition = new SpinState
        {
            SpinUp = new Complex(1 / Math.Sqrt(2), 0),
            SpinDown = new Complex(1 / Math.Sqrt(2), 0)
        };
        
        upResults = 0;
        for (int i = 0; i < 1000; i++)
        {
            var (spin, _) = QuantumMechanics.MeasureSpin(superposition);
            if (spin > 0) upResults++;
        }
        
        // Should be approximately 50/50
        Assert.InRange(upResults, 400, 600);
    }
    
    [Theory]
    [InlineData(0, Math.PI/4, 2.82842712)]  // Maximum violation
    [InlineData(0, 0, 2.0)]                  // No violation
    public void BellInequality_ShouldShowQuantumViolation(double angle1, double angle2, double expectedS)
    {
        var s = QuantumMechanics.CalculateBellParameter(angle1, angle2);
        Assert.Equal(expectedS, Math.Abs(s), 5);
    }
    
    [Fact]
    public void QuantumTunneling_ShouldDependOnBarrier()
    {
        var energy = 1.0;
        var barrierHeight = 2.0;
        var mass = 1.0;
        
        var thinBarrier = 0.1;
        var thickBarrier = 1.0;
        
        var ampThin = QuantumMechanics.GetTunnelingAmplitude(energy, barrierHeight, thinBarrier, mass);
        var ampThick = QuantumMechanics.GetTunnelingAmplitude(energy, barrierHeight, thickBarrier, mass);
        
        // Tunneling probability decreases with barrier width
        Assert.True(ampThin > ampThick);
        
        // Above barrier, should be 1
        var aboveBarrier = QuantumMechanics.GetTunnelingAmplitude(3.0, barrierHeight, thickBarrier, mass);
        Assert.Equal(1.0, aboveBarrier);
    }
    
    [Fact]
    public void ColorWaveFunction_Singlet_ShouldBeIdentified()
    {
        // Equal superposition of RGB (simplified singlet)
        var singlet = new ColorWaveFunction
        {
            RedAmplitude = new Complex(1 / Math.Sqrt(3), 0),
            GreenAmplitude = new Complex(1 / Math.Sqrt(3), 0),
            BlueAmplitude = new Complex(1 / Math.Sqrt(3), 0)
        };
        
        Assert.True(singlet.IsColorSinglet());
        
        // Non-singlet state
        var nonSinglet = new ColorWaveFunction
        {
            RedAmplitude = Complex.One,
            GreenAmplitude = Complex.Zero,
            BlueAmplitude = Complex.Zero
        };
        
        Assert.False(nonSinglet.IsColorSinglet());
    }
    
    [Fact]
    public void QuantumZenoEffect_ShouldSlowDecay()
    {
        var naturalDecayRate = 1.0;
        var measurementRate = 10.0;
        
        var effectiveRate = QuantumMechanics.GetZenoFactor(measurementRate, naturalDecayRate);
        
        // Frequent measurements should reduce decay rate
        Assert.True(effectiveRate < naturalDecayRate);
    }
}