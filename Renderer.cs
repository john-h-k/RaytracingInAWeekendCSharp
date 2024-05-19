// Image
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

public readonly record struct RenderParameters(int Width, int Height, int SamplesPerPixel, int MaxDepth);
public readonly record struct RenderResult(TimeSpan Duration);

public class Renderer
{
    public RenderParameters RenderParameters;
    public Camera Camera;
    public Image<RgbaVector> Image;
    public LinearizedBoundingVolumeHierarchy World;
    private Queue<Block> _blocks = new();

    public Renderer(RenderParameters renderParameters, Camera camera, LinearizedBoundingVolumeHierarchy world)
    {
        this.RenderParameters = renderParameters;
        this.Camera = camera;
        this.World = world;
        this.Image = new Image<RgbaVector>(this.RenderParameters.Width, this.RenderParameters.Height);
    }

    private readonly record struct Block(int Left, int Right, int Bottom, int Top);

    public RenderResult Render()
    {
        _blocks.Clear();

        // Render
        var stopwatch = Stopwatch.StartNew();

        const int blockSize = 25;
        _blocks = new Queue<Block>();

        for (var j = 0; j < RenderParameters.Height; j += blockSize)
        {
            for (var i = 0; i < RenderParameters.Width; i += blockSize)
            {
                _blocks.Enqueue(new Block(
                    Left: i,
                    Right: Math.Min(RenderParameters.Width, i + blockSize),
                    Bottom: j,
                    Top: Math.Min(RenderParameters.Height, j + blockSize)
                ));
            }
        }

        var cores = Environment.ProcessorCount;
        var tasks = new Task[cores];

        Thread.MemoryBarrier();

        for (var core = 0; core < cores; core++)
        {
            tasks[core] = Task.Run(RenderWorker);
        }

        Task.WhenAll(tasks).Wait();

        stopwatch.Stop();
        return new RenderResult(stopwatch.Elapsed);
    }

    
    private void RenderWorker()
    {
        Random.Initialize();

        for (var i = 0; i < int.MaxValue; i++)
        {
            if (!_blocks.TryDequeue(out var block))
            {
                return;
            }

            Console.Error.Write($"{this._blocks.Count} blocks remaining... \r");

            this.RenderBlock(block);
        }

    }

    private void RenderBlock(Block block)
    {
        for (var y = block.Bottom; y < block.Top; y++)
        {
            for (var x = block.Left; x < block.Right; x++)
            {
                this.RenderPixel(x, y);
            }
        }
    }

    private void RenderPixel(int x, int y)
    {
        var pixelColor = Vector3.Zero;

        for (int s = 0; s < RenderParameters.SamplesPerPixel; s++)
        {
            var u = (x + Random.RandomSingle()) / (RenderParameters.Width - 1);
            var v = ((RenderParameters.Height - y) + Random.RandomSingle()) / (RenderParameters.Height - 1);
            var r = Camera.GetRay(u, v);
            pixelColor += RayColor(r, World, RenderParameters.MaxDepth);
        }

        var color = new RgbaVector();
        color.FromVector4(Image[x, y].ToVector4() + ConvertToColorspace(pixelColor, RenderParameters.SamplesPerPixel));
        Image[x, y] = color;

        static Vector4 ConvertToColorspace(Color pixelColor, int samplesPerPixel)
        {
            // Divide the color by the number of samples and gamma-correct for gamma=2.0.
            var scale = 1.0f / samplesPerPixel;
            var scaled = Vector3.Clamp(Vector3.SquareRoot(scale * pixelColor), Vector3.Zero, new Vector3(0.999f));

            return new Vector4(scaled, 1);
        }
    }

    private static Color RayColor(Ray r, LinearizedBoundingVolumeHierarchy world, int depth)
    {
        // If we've exceeded the ray bounce limit, no more light is gathered.
        if (depth <= 0)
        {
            return Color.Zero;
        }

        HitRecord rec = default;
        if (world.Hit(r, 0.001f, float.PositiveInfinity, ref rec))
        {
            Ray scattered = default;
            Color attenuation = default;
            
            if (rec.Mat.Scatter(r, rec, ref attenuation, ref scattered))
            {
                return attenuation * RayColor(scattered, world, depth - 1);
            }

            return Color.Zero;
        }

        var unitDirection = Vector3.Normalize(r.Direction);
        var t = 0.5f * (unitDirection.Y + 1.0f);
        return new Color(1.0f - t) + (t * new Color(0.5f, 0.7f, 1.0f));
    }

}
