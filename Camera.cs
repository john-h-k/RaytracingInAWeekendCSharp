

public sealed class Camera
{
    public Camera(Point3 lookFrom, Point3 lookAt, Vector3 vUp, float vFov, float aspectRatio, float aperture, float focusDist)
    {
        var theta = vFov.ToRadians();
        var h = MathF.Tan(theta / 2);
        var viewportHeight = 2.0f * h;
        var viewportWidth = aspectRatio * viewportHeight;

        this.w = Vector3.Normalize(lookFrom - lookAt);
        this.u = Vector3.Normalize(Vector3.Cross(vUp, w));
        this.v = Vector3.Cross(w, u);

        this.origin = lookFrom;
        this.horizontal = (focusDist * viewportWidth) * this.u;
        this.vertical = (focusDist * viewportHeight) * this.v;
        this.lowerLeftCorner = this.origin - (this.horizontal / 2) - (this.vertical / 2) - ((float)focusDist * this.w);
    
        this.lensRadius = aperture / 2;
    }

    public Ray GetRay(float s, float t)
    {
        var rd = this.lensRadius * Random.RandomInUnitDisk();
        var offset = this.u * rd.X + this.v * rd.Y;

        return new Ray(
            origin + offset,
            this.lowerLeftCorner + (s * this.horizontal) + (t * this.vertical) - this. origin - offset
        );
    }

    private Point3 origin;
    private Point3 lowerLeftCorner;
    private Vector3 horizontal;
    private Vector3 vertical;
    private Vector3 u, v, w;
    private float lensRadius;
}