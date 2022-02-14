using System.Numerics;
using Messaging;

namespace Shared.Projectiles;

public class DaggerProjectile : Projectile
{
    public DaggerProjectile(float x, float y, Vector2 direction, float rotation) : 
        base(0, ProjectileType.Dagger, x, y, direction, 400f, rotation, 10)
    {
    }

    public override void HitEnemy(Enemy enemy, bool isServer)
    {
        if (isServer)
        {
            enemy.TakeDamage(Damage);
            Server.SendMessageToAll(Message.MessageType.EnemyDamage, new EnemyDamageData(enemy.Id, Damage));
        }
                
        Destroy();
    }
}