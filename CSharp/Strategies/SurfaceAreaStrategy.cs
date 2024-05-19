public struct SurfaceAreaStrategy : IBoundingVolumeSplitStrategy
{
    private const int BinCount = 16;
    private const int MaxObjectsPerTerminal = 8;
    private const int MinObjectsForSurfaceArea = 4;
    
    private record struct Bin(int Count, AxisAlignedBoundingBox BoundingBox);

    private static int BinIndex(Axis axis, in AxisAlignedBoundingBox centroidBoundingBox, Hittable hittable)
    {
        var offset = centroidBoundingBox.NormalizedOffset(hittable.BoundingBox.Center);
        var b = (int)(BinCount * axis switch {
            Axis.X => offset.X,
            Axis.Y => offset.Y,
            Axis.Z => offset.Z,
            _ => 0
        });

        return b == BinCount ? b - 1 : b;
    }

    private static float CostFn(
        float boundingSurfaceArea,
        int leftCount,
        in AxisAlignedBoundingBox left,
        int rightCount,
        in AxisAlignedBoundingBox right
    )
    {
        const float TraversalCost = 0.25f;

        var leftCost = leftCount * (left.SurfaceArea() / boundingSurfaceArea);
        var rightCost = rightCount * (right.SurfaceArea() / boundingSurfaceArea);

        return TraversalCost + leftCost + rightCost;
    }

    public void Partition(in SplitStrategyInfo info, ArraySegment<Hittable> objects, out ArraySegment<Hittable> left, out ArraySegment<Hittable> right)
    {
        if (objects.Count < MinObjectsForSurfaceArea)
        {
            default(EqualSubsetSplitStrategy).Partition(info, objects, out left, out right);
            return;
        }

        Span<Bin> bins = stackalloc Bin[BinCount];
        bins.Clear();
        
        var parentSurfaceArea = info.BoundingBox.SurfaceArea();

        foreach (var @object in objects)
        {
            ref var bin = ref bins[BinIndex(info.Axis, info.BoundingBox, @object)];

            bin.Count++;
            bin.BoundingBox = bin.BoundingBox.Union(@object.BoundingBox);
        }

        Span<float> costs = stackalloc float[BinCount];

        for (var i = 0; i < BinCount; i++)
        {
            var leftTotal = 0;
            var rightTotal = 0;

            var leftBoundingBox = AxisAlignedBoundingBox.Zero;
            var rightBoundingBox = AxisAlignedBoundingBox.Zero;

            for (var lo = 0; lo < i; lo++)
            {
                ref var bin = ref bins[lo];

                leftTotal += bin.Count;
                leftBoundingBox = leftBoundingBox.Union(bin.BoundingBox);
            }

            for (var hi = i; hi < BinCount; hi++)
            {
                ref var bin = ref bins[hi];

                rightTotal += bin.Count;
                rightBoundingBox = rightBoundingBox.Union(bin.BoundingBox);
            }

            costs[i] = CostFn(
                parentSurfaceArea,
                leftTotal,
                leftBoundingBox,
                rightTotal,
                rightBoundingBox
            );
            //Console.WriteLine(costs[i]);
           // Console.WriteLine(i);
        }

        var minCost = float.PositiveInfinity;
        var minCostIndex = -1;

        var index = 0;
        foreach (var cost in costs)
        {
            if (cost < minCost)
            {
                minCost = cost;
                minCostIndex = index;
            }

            index++;
        }

        var linearTraversalCost = objects.Count;

        if (objects.Count > MaxObjectsPerTerminal || minCost < linearTraversalCost)
        {
            var (axis, bounds) = (info.Axis, info.BoundingBox);

            var mid = objects.AsSpan().Partition(value => BinIndex(axis, bounds, value) < minCostIndex);

            //Console.WriteLine($"{mid} split from {objects.Count}; minCost={minCost} at index {minCostIndex}");

            if (mid == 0)
            {
                default(MidpointSplitStrategy).Partition(info, objects, out left, out right);
            }
            else 
            {
                left = objects.Slice(0, mid);
                right = objects.Slice(mid);
            }
        }
        else
        {
            left = objects;
            right = default;
        }
    }
}
