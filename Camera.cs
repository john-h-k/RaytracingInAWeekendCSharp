

public class Camera
{
    public Camera(Point3 lookFrom, Point3 lookAt, Vector3 vUp, double vFov, double aspectRatio, double aperture, double focusDist)
    {
        var theta = vFov.ToRadians();
        var h = Math.Tan(theta / 2);
        var viewportHeight = 2.0 * h;
        var viewportWidth = aspectRatio * viewportHeight;

        this.w = Vector3.Normalize(lookFrom - lookAt);
        this.u = Vector3.Normalize(Vector3.Cross(vUp, w));
        this.v = Vector3.Cross(w, u);

        this.origin = lookFrom;
        this.horizontal = (float)(focusDist * viewportWidth) * this.u;
        this.vertical = (float)(focusDist * viewportHeight) * this.v;
        this.lowerLeftCorner = this.origin - (this.horizontal / 2) - (this.vertical / 2) - ((float)focusDist * this.w);
    
        this.lensRadius = aperture / 2;
    }

    public Ray GetRay(double s, double t) {
        var rd = (float)this.lensRadius * Random.RandomInUnitDisk();
        var offset = u * rd.X + v * rd.Y;

        return new Ray(
            origin + offset,
            this.lowerLeftCorner + ((float)s * this.horizontal) + ((float)t * this.vertical) - this. origin - offset
        );
    }

    private Point3 origin;
    private Point3 lowerLeftCorner;
    private Vector3 horizontal;
    private Vector3 vertical;
    private Vector3 u, v, w;
    private double lensRadius;
}