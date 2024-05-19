public static partial class Scenes
{
    public static LinearizedBoundingVolumeHierarchy Spheres<TStrategy>()
        where TStrategy : struct, IBoundingVolumeSplitStrategy
    {
        var world = new List<Hittable>();

        var groundMaterial = new Lambertian(new Color(0.5f, 0.5f, 0.5f));
        world.Add(new Sphere(new Point3(0, -1000, 0), 1000, groundMaterial));

        for (int a = -11; a < 11; a++)
        {
            for (int b = -11; b < 11; b++)
            {
                var chooseMat = Random.RandomSingle();
                var center = new Point3(a + (0.9f * Random.RandomSingle()), 0.2f, b + (0.9f * Random.RandomSingle()));

                if ((center - new Point3(4, 0.2f, 0)).Length() > 0.9f)
                {
                    Material sphereMaterial;

                    if (chooseMat < 0.8)
                    {
                        // diffuse
                        var albedo = Random.RandomVector3() * Random.RandomVector3();
                        sphereMaterial = new Lambertian(albedo);
                        world.Add(new Sphere(center, 0.2f, sphereMaterial));
                    }
                    else if (chooseMat < 0.95)
                    {
                        // metal
                        var albedo = Random.RandomVector3(0.5f, 1);
                        var fuzz = Random.RandomSingle(0, 0.5f);
                        sphereMaterial = new Metal(albedo, fuzz);
                        world.Add(new Sphere(center, 0.2f, sphereMaterial));
                    }
                    else
                    {
                        // glass
                        sphereMaterial = new Dielectric(1.5f);
                        world.Add(new Sphere(center, 0.2f, sphereMaterial));
                    }
                }
            }
        }

        var material1 = new Dielectric(1.5f);
        world.Add(new Sphere(new Point3(0, 1, 0), 1.0f, material1));
        //world.Add(new Sphere(new Point3(0, 1, 0), -0.8f, material1));

        var material2 = new Lambertian(new Color(0.4f, 0.2f, 0.1f));
        world.Add(new Sphere(new Point3(-4, 1, 0), 1.0f, material2));

        var material3 = new Metal(new Color(0.7f, 0.6f, 0.5f), 0.0f);
        world.Add(new Sphere(new Point3(4, 1, 0), 1.0f, material3));

        return BoundingVolumeNode.Create<TStrategy>(world.ToArray()).Linearize();
    }
}
