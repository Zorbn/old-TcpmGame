using System.Numerics;

namespace Shared.Projectiles;

public class DaggerProjectile : Projectile
{
    public DaggerProjectile(float x, float y, Vector2 direction, float rotation) : base(0, ProjectileType.Dagger, x, y, direction, 400f, rotation)
    {
    }

    public override void Update(float frameTime, Quadtree quadtree)
    {
        base.Update(frameTime, quadtree);
        
        ReturnColliders.Clear();
        Collider projectileCollider = new(X, Y, Size, Size);
        quadtree.Retrieve(ref ReturnColliders, projectileCollider);

        foreach (Collider collider in ReturnColliders)
        {
            if (collider.CollidesWith(projectileCollider))
            {
                Destroy();
                break;
            }
        }
    }
}