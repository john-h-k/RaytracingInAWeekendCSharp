public static class FloatingPointExtensions
{
    public static double ToRadians(this double @this)
        => (@this * Math.PI) / 180;

    public static float ToRadians(this float @this)
        => (@this * MathF.PI) / 180;
}