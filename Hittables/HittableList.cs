
public sealed class HittableList : Hittable
{
    private Hittable[] objects;
    public HittableList(IEnumerable<Hittable> objects) : base(default(AxisAlignedBoundingBox))
    {
        this.objects = objects.ToArray();

        foreach (var @object in this.objects)
        {
            this.BoundingBox = this.BoundingBox.Union(@object.BoundingBox);
        }        
    }

    // public void Build()
    // {
    //     this.objects.Sort((l, r) => l.AxisAlignedBoundingBox.Volume().CompareTo(r.AxisAlignedBoundingBox.Volume()));
    
    // }

    public override bool Hit(in Ray r, float tMin, float tMax, ref HitRecord rec)
    {
        HitRecord tempRec = default;
        var hitAnything = false;
        var closestSoFar = tMax;

        foreach (var @object in this.objects)
        {
            if (@object.Hit(r, tMin, closestSoFar, ref tempRec)) 
            {
                hitAnything = true;
                closestSoFar = tempRec.T;
                rec = tempRec;
            }
        }

        return hitAnything;
    }
}