

public class Camera
{
    public Camera()
    {
        var aspectRatio = 16.0f / 9.0f;
        var viewportHeight = 2.0f;
        var viewportWidth = aspectRatio * viewportHeight;
        var focalLength = 1.0f;

        this.origin = new Vector3(0, 0, 0);
        this.horizontal = new Vector3(viewportWidth, 0.0f, 0.0f);
        this.vertical = new Vector3(0.0f, viewportHeight, 0.0f);
        this.lowerLeftCorner = this.origin - (horizontal / 2) - (vertical / 2) - new Vector3(0, 0, focalLength);
    }

    public Ray GetRay(double u, double v) {
        return new Ray(this.origin, this.lowerLeftCorner + ((float)u * this.horizontal) + ((float)v * this.vertical) - origin);
    }

    private Point3 origin;
    private Point3 lowerLeftCorner;
    private Vector3 horizontal;
    private Vector3 vertical;
}