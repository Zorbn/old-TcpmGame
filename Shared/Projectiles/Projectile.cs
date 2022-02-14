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

    public int OriginClientId { get; }
    public int TextureIndex { get; }
    public ProjectileType Type { get; }
    public Vector2 Direction { get;  }
    
    public float X { get; private set;  }
    public float Y { get; private set; }
    public float OffsetX { get; }
    public float OffsetY { get; }
    public float Speed { get; }
    public float Rotation { get; }
    public int Damage { get; }
    public float Lifetime { get; }
    public float Age { get; private set; }

    protected List<Collider> ReturnColliders;

    protected Projectile(int originClientId, int textureIndex, ProjectileType type, float x, float y, Vector2 direction,
        float speed, float rotation, int damage, float lifetime)
    {
        OriginClientId = originClientId;
        TextureIndex = textureIndex;
        Type = type;
        X = x;
        Y = y;
        Direction = direction;
        Speed = speed;
        Rotation = rotation;
        Damage = damage;
        Lifetime = lifetime;
        Age = 0f;
        
        ReturnColliders = new List<Collider>();

        OffsetX = 2 * OffsetRange * Random.NextSingle() - OffsetRange;
        OffsetY = 2 * OffsetRange * Random.NextSingle() - OffsetRange;
    }

    public virtual void Update(float frameTime, Quadtree quadtree, bool isServer)
    {
        Age += frameTime;

        if (Age > Lifetime)
        {
            Destroy();
            return;
        }
        
        X += Direction.X * Speed * frameTime;
        Y += Direction.Y * Speed * frameTime;
        
        ReturnColliders.Clear();
        Collider projectileCollider = new(X, Y, Size, Size, this);
        quadtree.Retrieve(ref ReturnColliders, projectileCollider);

        foreach (Collider collider in ReturnColliders)
        {
            if (collider.CollidesWith(projectileCollider) && collider.Owner is Enemy enemy)
            {
                HitEnemy(enemy, isServer);
                break;
            }
        }
    }

    public virtual void HitEnemy(Enemy enemy, bool isServer)
    {
        
    }

    public void Destroy()
    {
        Projectiles.Remove(this);
    }

    public static void UpdateAll(float frameTime, Quadtree quadtree, bool isServer)
    {
        for (int index = 0; index < Projectiles.Count; index++)
        {
            Projectile projectile = Projectiles[index];
            projectile.Update(frameTime, quadtree, isServer);
        }
    }

    public static Projectile NewProjectile(int originClientId, ProjectileType type, float x, float y, Vector2 direction, float rotation)
    {
        Vector2 normalizedDirection = direction / direction.Length();

        Projectile newProjectile = type switch
        {
            ProjectileType.Dagger => new DaggerProjectile(originClientId, x, y, normalizedDirection, rotation),
            ProjectileType.Shield => new Projectile(originClientId, 1, type, x, y, normalizedDirection, 0f, rotation, 0,
                3f),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        Projectiles.Add(newProjectile);
        
        return newProjectile;
    }
}