using System.Numerics;

namespace Universe.Abstractions.Physics;

public static class QuantumChromodynamics
{
    // QCD Constants
    public const double AlphaStrongZ = 0.1185; // Strong coupling at Z boson mass
    public const double LambdaQCD = 0.217; // QCD scale in GeV
    public const double StringTension = 1.0; // GeV/fm
    public const double QCDScale = 0.2; // GeV - confinement scale
    public const double HbarC = 0.197327; // GeV·fm (ℏc)
    
    // Color factors for SU(3)
    public const double CF = 4.0 / 3.0; // Casimir factor for fundamental representation
    public const double CA = 3.0; // Casimir factor for adjoint representation
    public const double TF = 0.5; // Trace normalization
    public const int Nc = 3; // Number of colors
    public const int Nf = 6; // Number of flavors
    
    // Calculate running coupling constant
    public static double GetRunningCoupling(double energyScale)
    {
        if (energyScale < LambdaQCD) return 1.0; // Strong coupling regime
        
        var b0 = (11.0 * CA - 2.0 * Nf) / (12.0 * Math.PI);
        var t = Math.Log(energyScale / LambdaQCD);
        return AlphaStrongZ / (1.0 + b0 * AlphaStrongZ * t);
    }
    
    // Calculate QCD potential between quarks
    public static double GetQCDPotential(double distance)
    {
        var alphaS = GetRunningCoupling(1.0 / distance);
        
        // Cornell potential: V(r) = -4/3 * αs/r + kr
        var coulombTerm = -CF * alphaS * HbarC / distance;
        var confinementTerm = StringTension * distance;
        
        return coulombTerm + confinementTerm;
    }
    
    // Calculate the strong force between quarks
    public static Vector3 CalculateStrongForce(QuarkState q1, QuarkState q2)
    {
        var separation = q2.Position - q1.Position;
        var distance = separation.Length();
        
        if (distance < 1e-15f) return Vector3.Zero;
        
        var alphaS = GetRunningCoupling(1.0 / distance);
        
        // Force = -dV/dr
        var coulombForce = CF * alphaS * HbarC / (distance * distance);
        var confinementForce = StringTension;
        
        // Total force magnitude (always positive for QCD)
        var forceMagnitude = coulombForce + confinementForce;
        var forceDirection = separation / distance;
        
        // Apply color factor
        var colorFactor = GetColorFactor(q1.Color, q2.Color);
        
        // For color-anticolor pairs (positive color factor), force is attractive
        // Force points along separation vector (from q1 to q2)
        // For same colors (negative color factor), force is repulsive
        if (colorFactor > 0)
        {
            // Attractive - force on q2 points away from q1
            return forceDirection * (float)forceMagnitude;
        }
        else
        {
            // Repulsive - force on q2 points towards q1  
            return -forceDirection * (float)forceMagnitude;
        }
    }
    
    // Calculate color factor for quark-quark interaction
    public static double GetColorFactor(ColorCharge c1, ColorCharge c2)
    {
        // Simplified color factor calculation
        // In reality, this involves SU(3) Casimir operators
        if (c1 == c2) return -CF / 2.0; // Same color: repulsive
        if (c1.GetAntiColor() == c2) return CF; // Color-anticolor: attractive
        return -CF / 6.0; // Different colors: weakly attractive
    }
    
    // Check if a gluon exchange is allowed by color conservation
    public static bool IsGluonExchangeAllowed(ColorCharge initialColor, ColorCharge finalColor, 
        ColorCharge gluonColor1, ColorCharge gluonColor2)
    {
        // Gluon carries color-anticolor pair
        // Initial color + gluon anticolor = final color + gluon color
        return gluonColor2 == initialColor && gluonColor1 == finalColor;
    }
    
    // Calculate gluon self-interaction strength
    public static double GetGluonSelfCoupling(double energyScale)
    {
        var alphaS = GetRunningCoupling(energyScale);
        return alphaS * CA; // Gluon self-coupling proportional to CA
    }
    
    // Determine if quarks can form a color-neutral bound state
    public static bool CanFormBoundState(params QuarkState[] quarks)
    {
        var colors = quarks.Select(q => q.Color).ToArray();
        return ColorChargeExtensions.IsColorNeutral(colors);
    }
    
    // Calculate the QCD beta function
    public static double BetaFunction(double alphaS, int loops = 1)
    {
        var b0 = (11.0 * CA - 2.0 * Nf) / (12.0 * Math.PI);
        var beta = -b0 * alphaS * alphaS;
        
        if (loops >= 2)
        {
            var b1 = (17.0 * CA * CA - 5.0 * CA * Nf - 3.0 * CF * Nf) / (24.0 * Math.PI * Math.PI);
            beta -= b1 * alphaS * alphaS * alphaS;
        }
        
        return beta;
    }
    
    // Calculate asymptotic freedom scale
    public static double GetAsymptoticFreedomScale(double distance)
    {
        if (distance > 1.0) return 0.0; // No asymptotic freedom at large distances
        
        // At very short distances, coupling becomes weak
        return Math.Exp(-distance / 0.1); // 0.1 fm characteristic scale
    }
    
    // Flux tube energy between quarks
    public static double GetFluxTubeEnergy(double distance)
    {
        return StringTension * distance; // Linear confinement
    }
    
    // Check if distance allows perturbative QCD
    public static bool IsPerturbativeRegime(double distance)
    {
        var energyScale = HbarC / distance; // Convert distance to energy scale
        var alphaS = GetRunningCoupling(energyScale);
        return alphaS < 0.4 && distance < 0.5; // Perturbative if coupling is small and distance is short
    }
}