public static class Vector3Extensions
{
    public static bool NearZero(this Vector3 @this)
    {
        // Return true if the vector is close to zero in all dimensions.
        var s = 1e-8;
        @this = Vector3.Abs(@this);
        return (@this.X < s) && (@this.Y < s) && (@this.Z < s);
    }
}
