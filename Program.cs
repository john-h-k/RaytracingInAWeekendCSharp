// Image

using System.Diagnostics;

const double aspectRatio = 3.0 / 2.0;
const int imageWidth = 1200;
const int imageHeight = (int)(imageWidth / aspectRatio);
const int samplesPerPixel = 500;
const int maxDepth = 50;

// World
var world = RandomScene();

// Camera
var lookFrom = new Point3(13, 2, 3);
var lookAt = new Point3(0, 0, 0);
var vUp = new Vector3(0, 1, 0);
var distToFocus = 10.0;
var aperture = 0.1;

var cam = new Camera(lookFrom, lookAt, vUp, 20, aspectRatio, aperture, distToFocus);

// Render

Console.WriteLine("P3");

Console.Write(imageWidth);
Console.Write(' ');
Console.Write(imageHeight);
Console.WriteLine();

Console.WriteLine("255");

var stopwatch = Stopwatch.StartNew();

var cores = Environment.ProcessorCount;
var partitionSize = (imageWidth + (cores - 1)) / cores;

var tasks = new Task[cores];

for (int j = imageHeight - 1; j >= 0; --j)
{
    Console.Error.Write($"\rScanlines remaining: {j} ");
    Console.Error.Flush();

    var scanline = new Color[imageWidth];

    for (var partitionIndex = 0; partitionIndex < cores; partitionIndex++)
    {
        var row = j;
        var partition = partitionIndex;

        tasks[partition] = Task.Run(() => {
            var start = partition * partitionSize;
            var end = Math.Min(start + partitionSize, imageWidth);

            var length = end - start;

            for (int i = 0; i < length; ++i)
            {
                var pixelColor = new Vector3(0, 0, 0);

                for (int s = 0; s < samplesPerPixel; ++s)
                {
                    var u = (start + i + Random.RandomSingle()) / (imageWidth - 1);
                    var v = (row + Random.RandomSingle()) / (imageHeight - 1);
                    var r = cam.GetRay(u, v);
                    pixelColor += RayColor(r, world, maxDepth);
                }

                scanline[start + i] += pixelColor;
            }
        });
    }

    await Task.WhenAll(tasks);

    foreach (var pixel in scanline)
    {
        WriteColor(Console.Out, pixel, samplesPerPixel);
    }
}

Console.Error.WriteLine("\nDone.");
Console.Error.WriteLine($"Render took {stopwatch.Elapsed.TotalSeconds}s");

Color RayColor(Ray r, HittableList world, int depth)
{
    // If we've exceeded the ray bounce limit, no more light is gathered.
    if (depth <= 0)
    {
        return new Color(0, 0, 0);
    }

    HitRecord rec = default;
    if (world.Hit(r, 0.001, double.PositiveInfinity, ref rec))
    {
        Ray scattered = default;
        Color attenuation = default;
        if (rec.Mat.Scatter(r, rec, ref attenuation, ref scattered))
        {
            return attenuation * RayColor(scattered, world, depth - 1);
        }
        return new Color(0, 0, 0);
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

HittableList RandomScene()
{
    var world = new HittableList();

    var groundMaterial = new Lambertian(new Color(0.5f, 0.5f, 0.5f));
    world.Add(new Sphere(new Point3(0,-1000,0), 1000, groundMaterial));

    for (int a = -11; a < 11; a++) {
        for (int b = -11; b < 11; b++) {
            var chooseMat = Random.RandomSingle();
            var center = new Point3(a + (0.9f * Random.RandomSingle()), 0.2f, b + (0.9f * Random.RandomSingle()));

            if ((center - new Point3(4, 0.2f, 0)).Length() > 0.9f) {
                Material sphereMaterial;

                if (chooseMat < 0.8) {
                    // diffuse
                    var albedo = Random.RandomVector3() * Random.RandomVector3();
                    sphereMaterial = new Lambertian(albedo);
                    world.Add(new Sphere(center, 0.2, sphereMaterial));
                } else if (chooseMat < 0.95) {
                    // metal
                    var albedo = Random.RandomVector3(0.5f, 1);
                    var fuzz = Random.RandomSingle(0, 0.5f);
                    sphereMaterial = new Metal(albedo, fuzz);
                    world.Add(new Sphere(center, 0.2, sphereMaterial));
                } else {
                    // glass
                    sphereMaterial = new Dielectric(1.5);
                    world.Add(new Sphere(center, 0.2, sphereMaterial));
                }
            }
        }
    }

    var material1 = new Dielectric(1.5);
    world.Add(new Sphere(new Point3(0, 1, 0), 1.0, material1));

    var material2 = new Lambertian(new Color(0.4f, 0.2f, 0.1f));
    world.Add(new Sphere(new Point3(-4, 1, 0), 1.0, material2));

    var material3 = new Metal(new Color(0.7f, 0.6f, 0.5f), 0.0);
    world.Add(new Sphere(new Point3(4, 1, 0), 1.0, material3));

    return world;
}