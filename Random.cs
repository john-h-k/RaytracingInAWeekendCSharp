
static class Random
{
    private readonly static System.Random random = new();

    public static float RandomSingle()
        => (float)Random.random.NextSingle();

    public static float RandomSingle(float lo, float hi)
        => lo + ((hi - lo) * Random.random.NextSingle());

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
}