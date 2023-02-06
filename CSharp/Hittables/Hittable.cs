
public abstract class Hittable
{
    public AxisAlignedBoundingBox BoundingBox { get; protected set; }

    public Hittable(AxisAlignedBoundingBox boundingBox)
    {
        this.BoundingBox = boundingBox;
    }

    public abstract bool Hit(in Ray r, float tMin, float tMax, ref HitRecord rec);
}
