namespace Shared;

public struct Collider
{
    public float X, Y;
    public float Width, Height;

    public Collider(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public bool CollidesWith(Collider other)
    {
        return X + Width >= other.X && X <= other.X + other.Width && Y + Height >= other.Y &&
               Y <= other.Y + other.Height;
    }

    public bool Equals(Collider other)
    {
        float tolerance = 0.1f;
        return Math.Abs(X - other.X) < tolerance && Math.Abs(Y - other.Y) < tolerance && Math.Abs(Width - other.Width) < tolerance && Math.Abs(Height - other.Height) < tolerance;
    }
}