namespace Universe.Abstractions.Physics;

public enum ColorCharge
{
    Red,
    Green,
    Blue,
    AntiRed,
    AntiGreen,
    AntiBlue
}

public static class ColorChargeExtensions
{
    public static bool IsAntiColor(this ColorCharge color) => color switch
    {
        ColorCharge.AntiRed or ColorCharge.AntiGreen or ColorCharge.AntiBlue => true,
        _ => false
    };

    public static ColorCharge GetAntiColor(this ColorCharge color) => color switch
    {
        ColorCharge.Red => ColorCharge.AntiRed,
        ColorCharge.Green => ColorCharge.AntiGreen,
        ColorCharge.Blue => ColorCharge.AntiBlue,
        ColorCharge.AntiRed => ColorCharge.Red,
        ColorCharge.AntiGreen => ColorCharge.Green,
        ColorCharge.AntiBlue => ColorCharge.Blue,
        _ => throw new ArgumentOutOfRangeException(nameof(color))
    };

    public static bool IsColorNeutral(params ColorCharge[] colors)
    {
        if (colors.Length == 0) return false;

        // Mesons: color + anticolor
        if (colors.Length == 2)
        {
            return colors[0].GetAntiColor() == colors[1] || colors[1].GetAntiColor() == colors[0];
        }

        // Baryons: red + green + blue or antired + antigreen + antiblue
        if (colors.Length == 3)
        {
            var hasRed = colors.Contains(ColorCharge.Red);
            var hasGreen = colors.Contains(ColorCharge.Green);
            var hasBlue = colors.Contains(ColorCharge.Blue);
            var hasAntiRed = colors.Contains(ColorCharge.AntiRed);
            var hasAntiGreen = colors.Contains(ColorCharge.AntiGreen);
            var hasAntiBlue = colors.Contains(ColorCharge.AntiBlue);

            return (hasRed && hasGreen && hasBlue) || (hasAntiRed && hasAntiGreen && hasAntiBlue);
        }

        return false;
    }
}