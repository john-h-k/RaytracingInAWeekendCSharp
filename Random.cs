
static class Random
{
    private static System.Random random = System.Random.Shared;

    public static float RandomSingle()
        => Random.random.NextSingle();

    public static float RandomSingle(float lo, float hi)
        => lo + ((hi - lo) * Random.random.NextSingle());

    public static float RandomInt32(int lo, int hi)
        => Random.random.Next(lo, hi);

    public static Vector3 RandomVector3()
        => new Vector3(RandomSingle(), RandomSingle(), RandomSingle());

    public static Vector3 RandomVector3(float lo, float hi)
        => new Vector3(RandomSingle(lo, hi), RandomSingle(lo, hi), RandomSingle(lo, hi));

    public static Vector3 RandomInUnitSphere()
    {
        while (true)
        {
            var p = RandomVector3(-1, 1);

            if (p.LengthSquared() >= 1)
            {
                continue;
            }

            return p;
        }
    }

    public static Vector3 RandomInUnitDisk()
    {
        while (true)
        {
            var p = new Vector3(Random.RandomSingle(-1, 1), Random.RandomSingle(-1, 1), 0);
            
            if (p.LengthSquared() >= 1)
            {
                continue;
            }

            return p;
        }
    }
}