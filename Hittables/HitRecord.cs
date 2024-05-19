public record struct HitRecord(Point3 Point, Vector3 Normal, float T, bool FrontFace, Material Mat)
{
    public void SetFaceNormal(in Ray r, Vector3 outwardNormal)
    {
        this.FrontFace = Vector3.Dot(r.Direction, outwardNormal) < 0;
        this.Normal = this.FrontFace ? outwardNormal : -outwardNormal;
    }
}
