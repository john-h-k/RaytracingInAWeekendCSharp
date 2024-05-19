

public sealed class Sphere : Hittable
{
    public Point3 Center { get; }
    public float Radius { get; }
    public Material Mat { get; }

    public Sphere(Point3 center, float radius, Material mat)
    : base(new AxisAlignedBoundingBox(
        Min: center - new Vector3((float)radius),
        Max: center + new Vector3((float)radius)
    ))
    {
        this.Center = center;
        this.Radius = radius;
        this.Mat = mat;
    }

    public override bool Hit(in Ray r, float tMin, float tMax, ref HitRecord rec)
    {
        Vector3 oc = r.Origin - this.Center;
        var a = r.Direction.LengthSquared();
        var halfB = Vector3.Dot(oc, r.Direction);
        var c = oc.LengthSquared() - (float)(this.Radius * this.Radius);

        if (c > 0 && halfB > 0)
        {
            return false;
        }

        var discriminant = (halfB * halfB) - (a * c);
        if (discriminant < 0)
        {
            return false;
        }

        var sqrtd = MathF.Sqrt(discriminant);

        // Find the nearest root that lies in the acceptable range.
        var root = (-halfB - sqrtd) / a;
        if (root < tMin || tMax < root)
        {
            root = (-halfB + sqrtd) / a;
            if (root < tMin || tMax < root)
            {
                return false;
            }
        }

        rec.T = root;
        rec.Point = r.At(rec.T);
        var outwardNormal = (rec.Point - this.Center) / (float)this.Radius;
        rec.SetFaceNormal(r, outwardNormal);
        rec.Mat = this.Mat;

        return true;
    }
}