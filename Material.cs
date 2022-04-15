public abstract record class Material
{
    public abstract bool Scatter(Ray rIn, in HitRecord rec, ref Color attenuation, ref Ray scattered);
}

public record class Lambertian(Color Albedo) : Material
{
    public override bool Scatter(Ray rIn, in HitRecord rec, ref Color attenuation, ref Ray scattered)
    {
        var scatterDirection = rec.Normal + Random.RandomInUnitSphere();

        // Catch degenerate scatter direction
        if (scatterDirection.NearZero())
        {
            scatterDirection = rec.Normal;
        }


        scattered = new Ray(rec.P, scatterDirection);
        attenuation = this.Albedo;
        return true;
    }
}

public record class Metal(Color Albedo) : Material
{
    public override bool Scatter(Ray rIn, in HitRecord rec, ref Color attenuation, ref Ray scattered)
    {
        var reflected = Vector3.Reflect(Vector3.Normalize(rIn.Direction), rec.Normal);
        scattered = new Ray(rec.P, reflected);
        attenuation = this.Albedo;
        return Vector3.Dot(scattered.Direction, rec.Normal) > 0;
    }
};