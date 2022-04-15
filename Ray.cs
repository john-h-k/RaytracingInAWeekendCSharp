global using System.Numerics;
global using Point3 = System.Numerics.Vector3;
global using Color = System.Numerics.Vector3;

public record struct Ray(Point3 Origin, Vector3 Direction)
{
    public Point3 At(double t) => this.Origin + ((float)t * this.Direction);
}
