using System.Numerics;

namespace Shared.Projectiles;

public class Projectile
{
    public static List<Projectile> Projectiles = new();
    public const int Size = 32;

    private const float OffsetRange = Size / 3f;
    private static Random Random = new();
    
    public enum ProjectileType
    {
        Dagger,
        Shield
    }

    public int TextureIndex { get; }
    public ProjectileType Type { get; }
    public Vector2 Direction { get;  }
    
    public float X { get; private set;  }
    public float Y { get; private set; }
    public float OffsetX { get; }
    public float OffsetY { get; }
    public float Speed { get; }
    public float Rotation { get; }

    protected List<Collider> ReturnColliders;
    
    protected Projectile(int textureIndex, ProjectileType type, float x, float y, Vector2 direction, float speed, float rotation)
    {
        TextureIndex = textureIndex;
        Type = type;
        X = x;
        Y = y;
        Direction = direction;
        Speed = speed;
        Rotation = rotation;
        
        ReturnColliders = new List<Collider>();

        OffsetX = 2 * OffsetRange * Random.NextSingle() - OffsetRange;
        OffsetY = 2 * OffsetRange * Random.NextSingle() - OffsetRange;
    }

    public virtual void Update(float frameTime, Quadtree quadtree)
    {
        X += Direction.X * Speed * frameTime;
        Y += Direction.Y * Speed * frameTime;
    }

    public void Destroy()
    {
        Projectiles.Remove(this);
    }

    public static void UpdateAll(float frameTime, Quadtree quadtree)
    {
        foreach (Projectile projectile in Projectiles.ToArray())
        {
            projectile.Update(frameTime, quadtree);
        }
    }

    public static Projectile NewProjectile(ProjectileType type, float x, float y, Vector2 direction, float rotation)
    {
        Vector2 normalizedDirection = direction / direction.Length();
        
        Projectile newProjectile = type switch
        {
            ProjectileType.Dagger => new DaggerProjectile(x, y, normalizedDirection, rotation),
            ProjectileType.Shield => new Projectile(1, type, x, y, normalizedDirection, 0f, rotation),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        Projectiles.Add(newProjectile);
        
        return newProjectile;
    }
}