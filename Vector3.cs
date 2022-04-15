public static class Vector3Extensions
{
    public static bool NearZero(this Vector3 @this)
    {
        // Return true if the vector is close to zero in all dimensions.
        var s = 1e-8;
        return (MathF.Abs(@this.X) < s) && (MathF.Abs(@this.Y) < s) && (MathF.Abs(@this.Z) < s);
    }
}
