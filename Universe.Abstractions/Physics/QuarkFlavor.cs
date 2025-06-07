namespace Universe.Abstractions.Physics;

public enum QuarkFlavor
{
    Up,
    Down,
    Charm,
    Strange,
    Top,
    Bottom
}

public static class QuarkFlavorExtensions
{
    public static double GetMass(this QuarkFlavor flavor) => flavor switch
    {
        QuarkFlavor.Up => 2.2e-3,        // 2.2 MeV/c²
        QuarkFlavor.Down => 4.7e-3,      // 4.7 MeV/c²
        QuarkFlavor.Charm => 1.28,       // 1.28 GeV/c²
        QuarkFlavor.Strange => 0.096,    // 96 MeV/c²
        QuarkFlavor.Top => 173.1,        // 173.1 GeV/c²
        QuarkFlavor.Bottom => 4.18,      // 4.18 GeV/c²
        _ => throw new ArgumentOutOfRangeException(nameof(flavor))
    };

    public static double GetElectricCharge(this QuarkFlavor flavor) => flavor switch
    {
        QuarkFlavor.Up or QuarkFlavor.Charm or QuarkFlavor.Top => 2.0 / 3.0,
        QuarkFlavor.Down or QuarkFlavor.Strange or QuarkFlavor.Bottom => -1.0 / 3.0,
        _ => throw new ArgumentOutOfRangeException(nameof(flavor))
    };

    public static bool IsAntiQuark(this QuarkFlavor flavor, bool isAntiParticle) => isAntiParticle;
}