using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

public sealed class LinearizedBoundingVolumeHierarchy : Hittable
{
    [StructLayout(LayoutKind.Auto)]
    public record struct Node(
        AxisAlignedBoundingBox BoundingBox,
        Hittable? Hittable,
        int RightIndex,
        Axis SplitAxis
    );

    private Node[] nodes;

    public LinearizedBoundingVolumeHierarchy(IEnumerable<Node> nodes) : base(AxisAlignedBoundingBox.Inf)
    {
        this.nodes = nodes.ToArray();
    }

    public override bool Hit(in Ray r, float tMin, float tMax, ref HitRecord rec)
        => this.HitTestAtIndex(r, tMin, tMax, ref rec);

    public bool HitTestAtIndex_(int head, in Ray r, float tMin, float tMax, ref HitRecord rec)
    {
        ref var node = ref this.GetNode(head);

        if (node.Hittable != null)
        {
            return node.Hittable.Hit(r, tMin, tMax, ref rec);
        }

        var leftIndex = head + 1;
        var rightIndex = node.RightIndex;

        ref var left = ref this.GetNode(leftIndex);
        ref var right = ref this.GetNode(rightIndex);

        var leftBoundHit = left.BoundingBox.Intersects(r, tMin, tMax, out var leftT);
        var rightBoundHit = right.BoundingBox.Intersects(r, tMin, tMax, out var rightT);

        if (!leftBoundHit && !rightBoundHit)
        {
            return false;
        }

        int closerIndex, furtherIndex;
        if (leftT < rightT)
        {
            closerIndex = leftIndex;
            furtherIndex = rightIndex;
        }
        else
        {
            closerIndex = rightIndex;
            furtherIndex = leftIndex;
        }

        var hitClose = this.HitTestAtIndex_(leftIndex, r, tMin, tMax, ref rec);
        var hitFar = this.HitTestAtIndex_(rightIndex, r, tMin, hitClose ? rec.T : tMax, ref rec);

        return hitClose || hitFar;
    }

    public unsafe bool HitTestAtIndex(in Ray r, float tMin, float tMax, ref HitRecord rec)
    {
        const int MaxDepth = 64;
        var head = 0;
        var visitStack = stackalloc int[MaxDepth];
        var visitHead = 0;
        var hit = false;
        var raySign = AdvSimd.CompareGreaterThan(Vector128.AsVector128(r.Direction), Vector128<float>.Zero).AsInt32();
        
        while (true)
        {
            ref var node = ref this.GetNode(head);

            if (!node.BoundingBox.Intersects(r, tMin, tMax))
            {
                goto VisitNext;
            }
            else if (node.Hittable == null)
            {
                if (raySign.GetElement((int)node.SplitAxis) != 0)
                {
                    visitStack[visitHead++] = node.RightIndex;
                    head++;
                }
                else
                {
                    visitStack[visitHead++] = head + 1;
                    head = node.RightIndex;
                }

                continue;
            }
            else if (node.Hittable.Hit(r, tMin, tMax, ref rec))
            {
                hit = true;
                tMax = rec.T;
            }


        VisitNext:
            if (visitHead == 0)
            {
                return hit;
            }

            head = visitStack[--visitHead];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref Node GetNode(int index)
        =>
        #if DEBUG
        ref this.nodes[index];
        #else
        ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(this.nodes), index);
        #endif
}

public sealed class BoundingVolumeNode : Hittable
{
    public Axis SplitAxis { get; }
    public Hittable Left { get; }
    public Hittable Right { get; }

    private BoundingVolumeNode(Axis splitAxis, Hittable left, Hittable right) : base(left.BoundingBox.Union(right.BoundingBox))
    {
        this.SplitAxis = splitAxis;
        this.Left = left;
        this.Right = right;
    }

    private sealed record class NodeVisitInfo(Hittable Hittable, int ParentIndex);
    private sealed record class NodeBuildInfo(Hittable Hittable, int LeftIndex, int RightIndex);

    public LinearizedBoundingVolumeHierarchy Linearize()
    {
        var linear = new List<LinearizedBoundingVolumeHierarchy.Node>();

        Traverse(linear, this);

        return new LinearizedBoundingVolumeHierarchy(linear);

        static int Traverse(
            List<LinearizedBoundingVolumeHierarchy.Node> linear,
            Hittable node
        )
        {
            var offset = linear.Count;
            linear.Add(default);

            if (node is BoundingVolumeNode bvhNode)
            {   
                _= Traverse(linear, bvhNode.Left);

                linear[offset] = new LinearizedBoundingVolumeHierarchy.Node(
                    bvhNode.BoundingBox,
                    null,
                    Traverse(linear, bvhNode.Right),
                    bvhNode.SplitAxis
                );
            }
            else
            {
                linear[offset] = new LinearizedBoundingVolumeHierarchy.Node(
                    node.BoundingBox,
                    node,
                    -1,
                    default
                );
            }

            return offset;
        }
    }

    public static BoundingVolumeNode Create<TStrategy>(ArraySegment<Hittable> objects, TStrategy strategy = default) where TStrategy : struct, IBoundingVolumeSplitStrategy
    {
        var result = CreateNode(objects, strategy);

        if (result is BoundingVolumeNode bvh)
        {
            return bvh;
        }

        return new BoundingVolumeNode(Axis.X, result, new NeverHittable());
    }

    private static Hittable CreateNode<TStrategy>(ArraySegment<Hittable> objects, TStrategy strategy = default) where TStrategy : struct, IBoundingVolumeSplitStrategy
    {
        if (objects.Count == 1)
        {
            return objects[0];
        }
        else if (objects.Count == 2)
        {
            return new BoundingVolumeNode(Axis.X, objects[0], objects[1]);
        }

        var boundingBox = AxisAlignedBoundingBox.Zero;
        var centroidBoundingBox = AxisAlignedBoundingBox.Zero;

        foreach (var hittable in objects)
        {
            boundingBox = boundingBox.Union(hittable.BoundingBox);
            centroidBoundingBox = centroidBoundingBox.Union(hittable.BoundingBox.Center);
        }

        var dims = Vector3.Abs(centroidBoundingBox.Max - centroidBoundingBox.Min);

        Axis axis;
        if (dims.X > dims.Y && dims.X > dims.Z)
        {
            // X biggest
            axis = Axis.X;
        }
        else if (dims.Y > dims.X && dims.Y > dims.Z)
        {
            // Y biggest
            axis = Axis.Y;
        }
        else
        {
            // Z biggest
            axis = Axis.Z;
        }

        //Console.WriteLine($"Max axis '{axis}' with dimensions {dims}");

        var info = new SplitStrategyInfo(axis, boundingBox, centroidBoundingBox);
        strategy.Partition(info, objects, out var left, out var right);

        if (left.Count == 0 || right.Count == 0)
        {
            return new HittableList(left.Count == 0 ? right : left);
        }
        else
        {
            return new BoundingVolumeNode(
                axis,
                BoundingVolumeNode.CreateNode(left, strategy),
                BoundingVolumeNode.CreateNode(right, strategy)
            );
        }
    }

    public override bool Hit(in Ray r, float tMin, float tMax, ref HitRecord rec)
    {
        if (!this.BoundingBox.Intersects(r, tMin, tMax))
        {
            return false;
        }

        var hitLeft = this.Left.Hit(r, tMin, tMax, ref rec);
        var hitRight = this.Right.Hit(r, tMin, hitLeft ? rec.T : tMax, ref rec);

        return hitLeft || hitRight;
    }
}