global using System.Numerics;
global using Point3 = System.Numerics.Vector3;
global using Color = System.Numerics.Vector3;

public readonly record struct Ray(Point3 Origin, Vector3 Direction)
{
    public Vector3 InverseDirection { get; } = Vector3.One / Direction;

    public Point3 At(float t) => this.Origin + ((float)t * this.Direction);
}