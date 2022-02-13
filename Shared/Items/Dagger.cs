using Shared.Projectiles;

namespace Shared.Items;

public class Dagger : Item
{
    public Dagger(int textureIndex) : base(textureIndex, ItemType.Dagger, 0.5f)
    {
    }

    public override void Update(int playerId)
    {
        Player owner = Player.Players[playerId];
        float rotation = owner.Direction switch
        {
            Direction.Up => 270f,
            Direction.Down => 90f,
            Direction.Left => 180f,
            Direction.Right => 0f,
            _ => throw new ArgumentOutOfRangeException()
        };

        Projectile.NewProjectile(Projectile.ProjectileType.Dagger, owner.X, owner.Y,
            DirectionUtils.ToVector(owner.Direction), rotation);
    }
}