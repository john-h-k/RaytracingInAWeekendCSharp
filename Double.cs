public static class FloatingPointExtensions
{
    public static float ToRadians(this float @this)
        => (@this * MathF.PI) / 180;
}