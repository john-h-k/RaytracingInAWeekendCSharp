
record class HittableList : Hittable
{
    private List<Hittable> objects = new();

    public void Clear() => this.objects.Clear();
    public void Add(Hittable hittable) => this.objects.Add(hittable);

    public override bool Hit(in Ray r, double tMin, double tMax, ref HitRecord rec)
    {
        HitRecord tempRec = default;
        var hitAnything = false;
        var closestSoFar = tMax;

        foreach (var @object in objects) 
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