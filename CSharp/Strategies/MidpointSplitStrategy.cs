public struct MidpointSplitStrategy : IBoundingVolumeSplitStrategy
{
    const int MinPrimitivesPerInteriorNode = 1;

    public void Partition(in SplitStrategyInfo info, ArraySegment<Hittable> objects, out ArraySegment<Hittable> left, out ArraySegment<Hittable> right)
    {
        if (objects.Count < MinPrimitivesPerInteriorNode)
        {
            left = objects;
            right = default;
            return;
        }

        var midpoint = info.CentroidBoundingBox.Center;
        Predicate<Hittable> predicate = info.Axis switch
        {
            Axis.X => (l) => l.BoundingBox.Center.X < midpoint.X,
            Axis.Y => (l) => l.BoundingBox.Center.Y < midpoint.Y,
            Axis.Z => (l) => l.BoundingBox.Center.Z < midpoint.Z,
            _ => null!
        };

        var mid = objects.AsSpan().Partition(predicate);

        if (mid != 0 && mid != objects.Count)
        {
            left = objects.Slice(0, mid);
            right = objects.Slice(mid);
        }
        else
        {
            default(EqualSubsetSplitStrategy).Partition(info, objects, out left, out right);
        }
    }
}
