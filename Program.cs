// Image
var aspectRatio = 16.0f / 9.0f;
var imageWidth = 400;
var imageHeight = (int)(imageWidth / aspectRatio);
var samplesPerPixel = 100;
var maxDepth = 50;

// for (int i = 0; i < 50; i++)
// {
//     Console.WriteLine(Random.RandomSingle(-5, 5));
// }
// return;

// World
var world = new HittableList();
world.Add(new Sphere(new Point3(0, 0, -1), 0.5));
world.Add(new Sphere(new Point3(0, -100.5f ,-1), 100));

// Camera
var cam = new Camera();

Console.WriteLine("P3");

Console.Write(imageWidth);
Console.Write(' ');
Console.Write(imageHeight);
Console.WriteLine();

Console.WriteLine("255");

for (int j = imageHeight - 1; j >= 0; --j) 
{
    Console.Error.Write($"\rScanlines remaining: {j} ");
    Console.Error.Flush();

    for (int i = 0; i < imageWidth; ++i) 
    {   
        var pixelColor = new Vector3(0, 0, 0);
        for (int s = 0; s < samplesPerPixel; ++s)
        {
            var u = (i + Random.RandomSingle()) / (imageWidth - 1);
            var v = (j + Random.RandomSingle()) / (imageHeight - 1);
            var r = cam.GetRay(u, v);
            pixelColor += RayColor(r, world, maxDepth);
        }
        
        WriteColor(Console.Out, pixelColor, samplesPerPixel);
    }
}

Console.Error.WriteLine("\nDone.");

Color RayColor(Ray r, HittableList world, int depth)
{
    // If we've exceeded the ray bounce limit, no more light is gathered.
    if (depth <= 0)
        return new Color(0, 0, 0);

    HitRecord rec = default;
    if (world.Hit(r, 0.001, double.PositiveInfinity, ref rec)) {
        var target = rec.P + rec.Normal + Random.RandomInUnitSphere();
        return 0.5f * RayColor(new Ray(rec.P, target - rec.P), world, depth - 1);
    }

    var unitDirection = Vector3.Normalize(r.Direction);
    var t = 0.5f * (unitDirection.Y + 1.0f);
    return (1.0f - t) * new Color(1.0f, 1.0f, 1.0f) + (t * new Color(0.5f, 0.7f, 1.0f));
}

void WriteColor(TextWriter @out, Color pixelColor, int samplesPerPixel)
{
    var r = pixelColor.X;
    var g = pixelColor.Y;
    var b = pixelColor.Z;
    
    // Divide the color by the number of samples and gamma-correct for gamma=2.0.
    var scale = 1.0f / samplesPerPixel;
    r = MathF.Sqrt(scale * r);
    g = MathF.Sqrt(scale * g);
    b = MathF.Sqrt(scale * b);

    // Write the translated [0,255] value of each color component.
    @out.Write((int)(256 * Math.Clamp(r, 0.0f, 0.999f)));
    @out.Write(' ');
    @out.Write((int)(256 * Math.Clamp(g, 0.0f, 0.999f)));
    @out.Write(' ');
    @out.Write((int)(256 * Math.Clamp(b, 0.0f, 0.999f)));
    @out.WriteLine();
}