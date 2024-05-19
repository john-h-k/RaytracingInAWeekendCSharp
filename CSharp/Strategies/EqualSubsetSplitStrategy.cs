public struct EqualSubsetSplitStrategy : IBoundingVolumeSplitStrategy
{
    public void Partition(in SplitStrategyInfo info, ArraySegment<Hittable> objects, out ArraySegment<Hittable> left, out ArraySegment<Hittable> right)
    {
        Comparison<Hittable> comparer = info.Axis switch
        {
            Axis.X => (l, r) => l.BoundingBox.Min.X.CompareTo(r.BoundingBox.Min.X),
            Axis.Y => (l, r) => l.BoundingBox.Min.Y.CompareTo(r.BoundingBox.Min.Y),
            Axis.Z => (l, r) => l.BoundingBox.Min.Z.CompareTo(r.BoundingBox.Min.Z),
            _ => null!
        };

        var mid = objects.Count / 2;

        _ = objects.AsSpan().NthElement(mid, comparer);

        left = objects.Slice(0, mid);
        right = objects.Slice(mid);
    }
}
