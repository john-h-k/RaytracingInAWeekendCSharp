public abstract record class Material
{
    public abstract bool Scatter(Ray rIn, in HitRecord rec, ref Color attenuation, ref Ray scattered);

    public static Vector3 Refract(Vector3 uv, Vector3 n, double etaiOverEtat)
    {
        var cosTheta = MathF.Min(Vector3.Dot(-uv, n), 1.0f);
        var rOutPerp = (float)etaiOverEtat * (uv + (cosTheta * n));
        var rOutParallel = -MathF.Sqrt(MathF.Abs(1.0f - rOutPerp.LengthSquared())) * n;
        return rOutPerp + rOutParallel;
    }
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

public record class Metal(Color Albedo, double Fuzz) : Material
{
    private double _fuzz = Fuzz;

    public double Fuzz { get => Math.Min(_fuzz, 1); init => _fuzz = value; }

    public override bool Scatter(Ray rIn, in HitRecord rec, ref Color attenuation, ref Ray scattered)
    {
        var reflected = Vector3.Reflect(Vector3.Normalize(rIn.Direction), rec.Normal);
        scattered = new Ray(rec.P, reflected + ((float)this.Fuzz * Random.RandomInUnitSphere()));
        attenuation = this.Albedo;
        return Vector3.Dot(scattered.Direction, rec.Normal) > 0;
    }
};

public record class Dielectric(double Ir) : Material
{
    public override bool Scatter(Ray rIn, in HitRecord rec, ref Color attenuation, ref Ray scattered)
    {
        attenuation = new Color(1.0f, 1.0f, 1.0f);
        var refractionRatio = rec.FrontFace ? (1.0f / this.Ir) : this.Ir;

        var unitDirection = Vector3.Normalize(rIn.Direction);
        var cosTheta = MathF.Min(Vector3.Dot(-unitDirection, rec.Normal), 1.0f);
        var sinTheta = MathF.Sqrt(1.0f - (cosTheta * cosTheta));

        bool cannotRefract = refractionRatio * sinTheta > 1.0f;

        Vector3 direction;
        if (cannotRefract || Dielectric.reflectance(cosTheta, refractionRatio) > Random.RandomSingle())
        {
            direction = Vector3.Reflect(unitDirection, rec.Normal);
        }
        else
        {
            direction = Refract(unitDirection, rec.Normal, refractionRatio);
        }

        scattered = new Ray(rec.P, direction);
        return true;
    }

    private static double reflectance(double cosine, double refIdx)
    {
        // Use Schlick's approximation for reflectance.
        var r0 = (1 - refIdx) / (1 + refIdx);
        r0 = r0 * r0;
        return r0 + ((1 - r0) * Math.Pow(1 - cosine, 5));
    }
};