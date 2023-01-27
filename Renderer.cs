// Image
using System.Collections.Concurrent;
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
    private ConcurrentQueue<Block> blocks;

    public Renderer(RenderParameters renderParameters)
    {
        this.RenderParameters = renderParameters;
    }

    private readonly record struct Block(int Left, int Right, int Bottom, int Top);

    public RenderResult Render()
    {
        // Render
        var stopwatch = Stopwatch.StartNew();

        const int blockSize = 25;
        this.Image = new Image<RgbaVector>(this.RenderParameters.Width, this.RenderParameters.Height);
        this.blocks = new ConcurrentQueue<Block>();

        for (var j = 0; j < this.RenderParameters.Height; j += blockSize)
        {
            for (var i = 0; i < this.RenderParameters.Width; i += blockSize)
            {
                this.blocks.Enqueue(new Block(
                    Left: i,
                    Right: Math.Min(this.RenderParameters.Width, i + blockSize),
                    Bottom: j,
                    Top: Math.Min(this.RenderParameters.Height, j + blockSize)
                ));
            }
        }

        var cores = Environment.ProcessorCount;
        var tasks = new Task[cores];

        Thread.MemoryBarrier();

        for (var core = 0; core < cores; core++)
        {
            tasks[core] = Task.Run(this.RenderWorker);
        }

        Task.WhenAll(tasks).Wait();

        stopwatch.Stop();
        return new RenderResult(stopwatch.Elapsed);
    }

    
    private void RenderWorker()
    {
        for (var i = 0; i < int.MaxValue; i++)
        {
            if (!this.blocks.TryDequeue(out var block))
            {
                return;
            }

            Console.Error.Write($"{this.blocks.Count} blocks remaining... \r");

            this.RenderBlock(block);
        }
    }

    [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
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

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void RenderPixel(int x, int y)
    {
        var pixelColor = Vector3.Zero;

        for (int s = 0; s < this.RenderParameters.SamplesPerPixel; s++)
        {
            var u = (x + Random.RandomSingle()) / (this.RenderParameters.Width - 1);
            var v = ((this.RenderParameters.Height - y) + Random.RandomSingle()) / (this.RenderParameters.Height - 1);
            var r = this.Camera.GetRay(u, v);
            pixelColor += RayColor(r, this.World, this.RenderParameters.MaxDepth);
        }

        var color = new RgbaVector();
        color.FromVector4(this.Image[x, y].ToVector4() + ConvertToColorspace(pixelColor, this.RenderParameters.SamplesPerPixel));
        this.Image[x, y] = color;

        [MethodImpl(MethodImplOptions.NoInlining)]
        static Vector4 ConvertToColorspace(Color pixelColor, int samplesPerPixel)
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
            return new Vector4(
                Math.Clamp(r, 0.0f, 0.999f),
                Math.Clamp(g, 0.0f, 0.999f),
                Math.Clamp(b, 0.0f, 0.999f),
                1
            );
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