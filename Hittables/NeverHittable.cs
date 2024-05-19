public sealed class NeverHittable : Hittable
{
    public NeverHittable() : base(AxisAlignedBoundingBox.Zero)
    {
    }

    public override bool Hit(in Ray r, float tMin, float tMax, ref HitRecord rec)
    {
        return false;
    }
}