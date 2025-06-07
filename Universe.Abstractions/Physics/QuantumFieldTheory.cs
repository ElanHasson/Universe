using System.Numerics;

namespace Universe.Abstractions.Physics;

public static class QuantumFieldTheory
{
    // Fundamental constants
    public const double PlanckConstant = 6.62607015e-34; // J·s
    public const double ReducedPlanck = 1.054571817e-34; // ℏ = h/2π
    public const double SpeedOfLight = 299792458; // m/s
    public const double ElementaryCharge = 1.602176634e-19; // C
    public const double VacuumPermittivity = 8.8541878128e-12; // F/m
    
    // Natural units (ℏ = c = 1)
    public const double GeVtoMeter = 1.97327e-16; // Conversion factor
    public const double GeVtoSecond = 6.58212e-25; // Conversion factor
    
    // Quantum field operators
    public static Complex CreateQuarkField(Vector3 position, double time, QuarkState quark)
    {
        // Simplified Dirac field for spin-1/2 particles
        var k = quark.Momentum;
        var E = quark.Energy;
        var phase = Vector3.Dot(k, position) - E * time;
        
        // Plane wave solution with normalization
        var amplitude = 1.0 / Math.Sqrt(2 * E);
        return new Complex(amplitude * Math.Cos(phase), amplitude * Math.Sin(phase));
    }
    
    // Feynman propagator for virtual particles
    public static Complex FeynmanPropagator(Vector3 x1, Vector3 x2, double t1, double t2, double mass)
    {
        var dx = x2 - x1;
        var dt = t2 - t1;
        var distance2 = dx.LengthSquared() - dt * dt; // Spacetime interval
        
        // Green's function for Klein-Gordon equation
        var denominator = new Complex(distance2 + mass * mass, -1e-10);
        return 1.0 / denominator;
    }
    
    // Vacuum fluctuations
    public static double GetVacuumEnergyDensity(double cutoffScale)
    {
        // Simplified vacuum energy calculation
        // In reality, this diverges and requires regularization
        var density = Math.Pow(cutoffScale, 4) / (16 * Math.PI * Math.PI);
        return density;
    }
    
    // Path integral formulation
    public static double CalculateTransitionAmplitude(QuarkState initial, QuarkState final, double time)
    {
        // Simplified path integral - in reality, sum over all paths
        var action = CalculateAction(initial, final, time);
        return Math.Exp(-action); // e^(iS/ℏ) in natural units
    }
    
    private static double CalculateAction(QuarkState initial, QuarkState final, double time)
    {
        // Classical action for free particle
        var avgMomentum = (initial.Momentum + final.Momentum) / 2;
        var avgEnergy = (initial.Energy + final.Energy) / 2;
        var kineticEnergy = avgMomentum.LengthSquared() / (2 * initial.Mass);
        
        return (kineticEnergy - avgEnergy) * time;
    }
    
    // Quantum corrections
    public static double GetQuantumCorrection(double couplingConstant, int loopOrder)
    {
        // Perturbative expansion corrections
        var correction = 1.0;
        var alphaPower = couplingConstant;
        
        for (int n = 1; n <= loopOrder; n++)
        {
            correction += alphaPower * GetLoopFactor(n);
            alphaPower *= couplingConstant;
        }
        
        return correction;
    }
    
    private static double GetLoopFactor(int loops)
    {
        // Simplified loop corrections
        return Math.Pow(0.159, loops); // 1/(2π)^loops
    }
    
    // Renormalization group equations
    public static double RunCoupling(double coupling0, double scale0, double scale, 
        Func<double, double> betaFunction)
    {
        // Solve dg/d(log μ) = β(g)
        var logRatio = Math.Log(scale / scale0);
        var coupling = coupling0;
        
        // Simple Euler integration
        var steps = 100;
        var dt = logRatio / steps;
        
        for (int i = 0; i < steps; i++)
        {
            coupling += betaFunction(coupling) * dt;
        }
        
        return coupling;
    }
    
    // Effective field theory
    public static double GetEffectiveCoupling(double fundamentalCoupling, double energyScale, 
        double cutoffScale)
    {
        if (energyScale > cutoffScale)
            return 0; // Theory breaks down above cutoff
            
        // Wilson's approach - integrate out high-energy modes
        var suppression = Math.Exp(-energyScale / cutoffScale);
        return fundamentalCoupling * suppression;
    }
    
    // Spontaneous symmetry breaking
    public static bool IsSymmetryBroken(double fieldValue, double criticalValue)
    {
        return Math.Abs(fieldValue) > criticalValue;
    }
    
    // Quantum tunneling probability
    public static double TunnelingProbability(double barrierHeight, double particleEnergy, 
        double barrierWidth)
    {
        if (particleEnergy > barrierHeight)
            return 1.0; // Classical passage
            
        var kappa = Math.Sqrt(2 * (barrierHeight - particleEnergy));
        return Math.Exp(-2 * kappa * barrierWidth);
    }
    
    // Zero-point energy
    public static double GetZeroPointEnergy(double frequency)
    {
        return 0.5 * frequency; // E₀ = ½ℏω (ℏ=1)
    }
    
    // Casimir effect between parallel plates
    public static double CasimirForce(double plateArea, double plateSeparation)
    {
        // F = -π²ℏc/(240d⁴) * Area
        var force = -Math.PI * Math.PI / (240 * Math.Pow(plateSeparation, 4)) * plateArea;
        return force;
    }
}