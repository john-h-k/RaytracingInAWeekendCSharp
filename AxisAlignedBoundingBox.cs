
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

public readonly record struct AxisAlignedBoundingBox(Vector3 Min, Vector3 Max)
{
    public static AxisAlignedBoundingBox Zero { get; } = default;
    public static AxisAlignedBoundingBox Inf { get; } = new AxisAlignedBoundingBox(new Vector3(float.NegativeInfinity), new Vector3(float.PositiveInfinity));

    public Point3 Center => this.Min + ((this.Max - this.Min) / 2);

    public float SurfaceArea()
    {
        var length = Vector3.Abs(this.Max - this.Min);
        
        return 2 * ((length.X * length.Y) + (length.X * length.Z) + (length.Y * length.Z));
    }

    public Vector3 NormalizedOffset(Point3 point)
    {
        return Vector3.Normalize(point - this.Min);
    }

    public AxisAlignedBoundingBox Union(in AxisAlignedBoundingBox other)
    {
        return new (
            Min: Vector3.Min(this.Min, other.Min),
            Max: Vector3.Max(this.Max, other.Max)
        );
    }

        public AxisAlignedBoundingBox Union(Point3 point)
    {
        return new (
            Min: Vector3.Min(this.Min, point),
            Max: Vector3.Max(this.Max, point)
        );
    }

    public bool Intersects(in Ray r, float tMin, float tMax)
    {
        var t0s = (this.Min - r.Origin) * r.InverseDirection;
        var t1s = (this.Max - r.Origin) * r.InverseDirection;
        
        var smaller = Vector3.Min(t0s, t1s);
        var bigger = Vector3.Max(t0s, t1s);

        var vMin = Vector128.AsVector128(new Vector4(smaller, tMin));
        var vMax = Vector128.AsVector128(new Vector4(bigger, tMax));

        return AdvSimd.Arm64.MaxAcross(vMin).GetElement(0) < AdvSimd.Arm64.MinAcross(vMax).GetElement(0);
    }

    public bool Intersects(in Ray r, float tMin, float tMax, out float t)
    {
        var t0s = (this.Min - r.Origin) * r.InverseDirection;
        var t1s = (this.Max - r.Origin) * r.InverseDirection;
        
        var smaller = Vector3.Min(t0s, t1s);
        var bigger = Vector3.Max(t0s, t1s);

        tMin = Math.Max(tMin, AdvSimd.Arm64.MaxAcross(Vector128.AsVector128(new Vector4(smaller, float.NegativeInfinity))).GetElement(0));
        tMax = Math.Min(tMax, AdvSimd.Arm64.MinAcross(Vector128.AsVector128(new Vector4(bigger, float.PositiveInfinity))).GetElement(0));

        var hit = tMin < tMax;
        t = hit ? tMin : float.PositiveInfinity;
        return hit;
    }

    // public bool Intersects(in Ray r, float tMin, float tMax, ref HitRecord rec)
    // {
    //     var invDirection = Vector3.One / r.Direction;

    //     var t0s = (this.Min - r.Origin) * invDirection;
    //     var t1s = (this.Max - r.Origin) * invDirection;
        
    //     var smaller = Vector3.Min(t0s, t1s);
    //     var bigger = Vector3.Max(t0s, t1s);

    //     var tNear = Math.Max(tMin, Math.Max(smaller.X, Math.Max(smaller.Y, smaller.Z)));
    //     var tFar = Math.Min(tMax, Math.Min(bigger.X, Math.Min(bigger.Y, bigger.Z)));

    //     if (tNear < tFar)
    //     {
    //         var hit = r.At(tNear);

    //         var plane = hit - this.Min;
    //         var outwardNormal = ;

    //         rec.Point = hit;
    //         rec.SetFaceNormal(r, outwardNormal);
    //         rec.T = tNear;
    //         return true;
    //     }
    //     else
    //     {
    //         return false;
    //     }
    // }
}
