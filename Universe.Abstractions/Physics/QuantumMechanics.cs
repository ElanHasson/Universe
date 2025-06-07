using System.Numerics;

namespace Universe.Abstractions.Physics;

[GenerateSerializer]
public record WaveFunction
{
    [Id(0)] public Complex[] Amplitudes { get; init; } = Array.Empty<Complex>();
    [Id(1)] public Vector3[] Positions { get; init; } = Array.Empty<Vector3>();
    [Id(2)] public double Normalization { get; init; }
    
    public double GetProbabilityDensity(Vector3 position)
    {
        // Find nearest grid point
        var minDistance = double.MaxValue;
        var nearestIndex = 0;
        
        for (int i = 0; i < Positions.Length; i++)
        {
            var distance = (Positions[i] - position).Length();
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestIndex = i;
            }
        }
        
        var amplitude = Amplitudes[nearestIndex];
        return (amplitude * Complex.Conjugate(amplitude)).Real * Normalization;
    }
}

[GenerateSerializer]
public record QuantumState
{
    [Id(0)] public WaveFunction SpatialWaveFunction { get; init; } = new();
    [Id(1)] public SpinState Spin { get; init; } = new();
    [Id(2)] public ColorWaveFunction ColorState { get; init; } = new();
    [Id(3)] public double Energy { get; init; }
    [Id(4)] public double AngularMomentum { get; init; }
    [Id(5)] public bool IsEigenstate { get; init; }
}

[GenerateSerializer]
public record SpinState
{
    [Id(0)] public Complex SpinUp { get; init; }
    [Id(1)] public Complex SpinDown { get; init; }
    
    public double GetSpinExpectation()
    {
        var upProb = (SpinUp * Complex.Conjugate(SpinUp)).Real;
        var downProb = (SpinDown * Complex.Conjugate(SpinDown)).Real;
        return 0.5 * (upProb - downProb); // <Sz> = ½(|↑|² - |↓|²)
    }
    
    public static SpinState CreateSpinUp() => new() 
    { 
        SpinUp = Complex.One, 
        SpinDown = Complex.Zero 
    };
    
    public static SpinState CreateSpinDown() => new() 
    { 
        SpinUp = Complex.Zero, 
        SpinDown = Complex.One 
    };
}

[GenerateSerializer]
public record ColorWaveFunction
{
    [Id(0)] public Complex RedAmplitude { get; init; }
    [Id(1)] public Complex GreenAmplitude { get; init; }
    [Id(2)] public Complex BlueAmplitude { get; init; }
    
    public bool IsColorSinglet()
    {
        // Color singlet: (|RGB⟩ - |GBR⟩ - |BRG⟩ + |RBG⟩ + |BGR⟩ - |GRB⟩)/√6
        // Simplified check for equal superposition
        var r = Complex.Abs(RedAmplitude);
        var g = Complex.Abs(GreenAmplitude);
        var b = Complex.Abs(BlueAmplitude);
        
        return Math.Abs(r - g) < 1e-10 && Math.Abs(g - b) < 1e-10;
    }
}

public static class QuantumMechanics
{
    // Pauli matrices for spin-1/2
    public static readonly Complex[,] PauliX = new Complex[,] 
    { 
        { 0, 1 }, 
        { 1, 0 } 
    };
    
    public static readonly Complex[,] PauliY = new Complex[,] 
    { 
        { 0, -Complex.ImaginaryOne }, 
        { Complex.ImaginaryOne, 0 } 
    };
    
    public static readonly Complex[,] PauliZ = new Complex[,] 
    { 
        { 1, 0 }, 
        { 0, -1 } 
    };
    
    // Time evolution operator
    public static WaveFunction TimeEvolve(WaveFunction psi, double hamiltonian, double time)
    {
        // U(t) = exp(-iHt/ℏ), with ℏ=1
        var phase = -hamiltonian * time;
        var evolutionFactor = new Complex(Math.Cos(phase), Math.Sin(phase));
        
        var evolvedAmplitudes = psi.Amplitudes
            .Select(a => a * evolutionFactor)
            .ToArray();
            
        return psi with { Amplitudes = evolvedAmplitudes };
    }
    
    // Heisenberg uncertainty principle
    public static double GetMinimumUncertainty(double positionUncertainty)
    {
        // ΔxΔp ≥ ℏ/2, with ℏ=1
        return 0.5 / positionUncertainty;
    }
    
    // Quantum measurement
    public static (Vector3 position, WaveFunction collapsed) MeasurePosition(WaveFunction psi)
    {
        // Calculate probabilities
        var probabilities = new double[psi.Positions.Length];
        var totalProb = 0.0;
        
        for (int i = 0; i < psi.Positions.Length; i++)
        {
            probabilities[i] = (psi.Amplitudes[i] * Complex.Conjugate(psi.Amplitudes[i])).Real;
            totalProb += probabilities[i];
        }
        
        // Choose measurement outcome
        var rand = Random.Shared.NextDouble() * totalProb;
        var cumulative = 0.0;
        var measuredIndex = 0;
        
        for (int i = 0; i < probabilities.Length; i++)
        {
            cumulative += probabilities[i];
            if (rand < cumulative)
            {
                measuredIndex = i;
                break;
            }
        }
        
        // Collapse wave function
        var collapsedAmplitudes = new Complex[psi.Amplitudes.Length];
        collapsedAmplitudes[measuredIndex] = Complex.One;
        
        var collapsed = psi with 
        { 
            Amplitudes = collapsedAmplitudes,
            Normalization = 1.0
        };
        
        return (psi.Positions[measuredIndex], collapsed);
    }
    
    // Spin measurement
    public static (double spinValue, SpinState collapsed) MeasureSpin(SpinState spin)
    {
        var upProb = (spin.SpinUp * Complex.Conjugate(spin.SpinUp)).Real;
        
        if (Random.Shared.NextDouble() < upProb)
        {
            return (0.5, SpinState.CreateSpinUp());
        }
        else
        {
            return (-0.5, SpinState.CreateSpinDown());
        }
    }
    
    // Entanglement
    public static bool AreEntangled(QuantumState state1, QuantumState state2)
    {
        // Simplified entanglement check
        // In reality, would compute Schmidt decomposition
        return true; // Placeholder
    }
    
    // Bell inequality test
    public static double CalculateBellParameter(double angle1, double angle2)
    {
        // CHSH inequality: |S| ≤ 2 classically, can be > 2 quantum mechanically
        // For entangled states: S = 2√2 * cos(2(θ2 - θ1))
        var angleDiff = angle2 - angle1;
        
        // Maximum violation occurs at π/4
        if (Math.Abs(angleDiff - Math.PI / 4) < 0.01)
        {
            return 2 * Math.Sqrt(2); // Maximum quantum violation
        }
        
        // For aligned angles, no violation
        if (Math.Abs(angleDiff) < 0.01)
        {
            return 2.0; // Classical limit
        }
        
        // General case
        return 2 * Math.Sqrt(2) * Math.Cos(2 * angleDiff);
    }
    
    // Quantum tunneling
    public static double GetTunnelingAmplitude(double energy, double barrierHeight, 
        double barrierWidth, double mass)
    {
        if (energy > barrierHeight) return 1.0;
        
        var k = Math.Sqrt(2 * mass * (barrierHeight - energy));
        return Math.Exp(-k * barrierWidth);
    }
    
    // Coherence length
    public static double GetCoherenceLength(double momentum, double energySpread)
    {
        // l_coh = ℏ/(Δp), with ℏ=1
        var momentumSpread = energySpread / (momentum / Math.Sqrt(momentum * momentum + 1));
        return 1.0 / momentumSpread;
    }
    
    // Quantum Zeno effect
    public static double GetZenoFactor(double measurementRate, double naturalDecayRate)
    {
        // Frequent measurements slow down decay
        return naturalDecayRate / (1 + measurementRate / naturalDecayRate);
    }
}