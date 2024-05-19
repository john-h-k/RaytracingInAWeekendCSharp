public enum Axis
{
    X,
    Y,
    Z
}

public readonly record struct SplitStrategyInfo(Axis Axis, AxisAlignedBoundingBox BoundingBox, AxisAlignedBoundingBox CentroidBoundingBox);

public interface IBoundingVolumeSplitStrategy
{
    void Partition(in SplitStrategyInfo info, ArraySegment<Hittable> objects, out ArraySegment<Hittable> left, out ArraySegment<Hittable> right);
}
