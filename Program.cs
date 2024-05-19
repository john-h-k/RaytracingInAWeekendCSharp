
using SixLabors.ImageSharp;

Random.Initialize();

// Image
var parameters = new RenderParameters
{
    Width = 1200,
    Height = 800,
    SamplesPerPixel = 500,
    MaxDepth = 50
};

// World

bool useEqSubset = false;


LinearizedBoundingVolumeHierarchy world;

if (useEqSubset)
{
    Console.Error.WriteLine($"Using {nameof(EqualSubsetSplitStrategy)}");
    world = Scenes.Spheres<EqualSubsetSplitStrategy>();
}
else
{
    Console.Error.WriteLine($"Using {nameof(MidpointSplitStrategy)}");
    world = Scenes.Spheres<MidpointSplitStrategy>();
}

// Camera
var lookFrom = new Point3(13, 2, 3);
var lookAt = new Point3(0, 0, 0);
var vUp = new Vector3(0, 1, 0);
var distToFocus = 10f;
var aperture = 0.1f;

var camera = new Camera(lookFrom, lookAt, vUp, 20, (float)parameters.Width / parameters.Height, aperture, distToFocus);

var renderer = new Renderer(parameters, camera, world);

var result = renderer.Render();

renderer.Image.SaveAsPng("output.png");
Console.Error.WriteLine("\nDone.");
Console.Error.WriteLine($"Render took {result.Duration.TotalSeconds}s");