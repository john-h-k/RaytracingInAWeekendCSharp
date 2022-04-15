
public record struct HitRecord(Point3 P, Vector3 Normal, double T, bool FrontFace, Material Mat)
{
    public void SetFaceNormal(in Ray r, Vector3 outwardNormal) {
        this.FrontFace = Vector3.Dot(r.Direction, outwardNormal) < 0;
        this.Normal = this.FrontFace ? outwardNormal : -outwardNormal;
    }
}

public abstract record class Hittable
{
    public abstract bool Hit(in Ray r, double tMin, double tMax, ref HitRecord rec);
}

public sealed record class Sphere(Point3 Center, double Radius, Material Mat) : Hittable
{
    public override bool Hit(in Ray r, double tMin, double tMax, ref HitRecord rec)
    {   
        Vector3 oc = r.Origin - this.Center;
        var a = r.Direction.LengthSquared();
        var halfB = Vector3.Dot(oc, r.Direction);
        var c = oc.LengthSquared() - (float)(this.Radius * this.Radius);

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
        rec.P = r.At(rec.T);
        var outwardNormal = (rec.P - this.Center) / (float)this.Radius;
        rec.SetFaceNormal(r, outwardNormal);
        rec.Mat = this.Mat;

        return true;
    }
}