namespace Shared;

public struct Collider
{
    public float X, Y;
    public float Width, Height;
    public object? Owner;

    public Collider(float x, float y, float width, float height, object? owner)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
        Owner = owner;
    }

    public bool CollidesWith(Collider other)
    {
        if (Owner != null && other.Owner != null && other.Owner == Owner) return false; // Don't collide with self
        
        return X + Width >= other.X && X <= other.X + other.Width && Y + Height >= other.Y &&
               Y <= other.Y + other.Height;
    }

    public bool Equals(Collider other)
    {
        float tolerance = 0.1f;
        return Math.Abs(X - other.X) < tolerance && Math.Abs(Y - other.Y) < tolerance && Math.Abs(Width - other.Width) < tolerance && Math.Abs(Height - other.Height) < tolerance;
    }
}