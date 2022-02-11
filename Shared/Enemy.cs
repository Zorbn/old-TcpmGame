using Messaging;

namespace Shared;

public class Enemy : SyncedEntity
{
    public enum EnemyType
    {
        Default
    }

    public static Dictionary<int, Enemy> Enemies = new();
    private static int NextId;

    public int Id;
    public EnemyType Type;
    public int Damage;
    public float Speed;
    public int Size;
    public const float DefaultSpeed = 100f;

    private static float AttackDelay = 0.5f;
    private float attackTimer;

    public static Enemy NewEnemy(EnemyType type, float x, float y, int damage, float speed = DefaultSpeed, int size = 32)
    {
        return type switch
        {
            EnemyType.Default => new Enemy(x, y, damage, speed, size),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

    }
    
    private Enemy(float x, float y, int damage, float speed = DefaultSpeed, int size = 20) : base(x, y)
    {
        Type = EnemyType.Default;
        Speed = speed;
        Size = size;
        Damage = damage;
    }

    public void ChaseNearestPlayer(float tickTime, Quadtree quadtree, List<Collider> returnColliders)
    {
        float nearestPlayerDistSqr = float.MaxValue;
        int nearestPlayerId = -1;
        
        foreach ((int playerId, Player player) in Player.Players)
        {
            float distX = X - player.X;
            float distY = Y - player.Y;
            float distSqr = distX * distX + distY * distY;

            if (!(distSqr < nearestPlayerDistSqr)) continue;
            
            nearestPlayerDistSqr = distSqr;
            nearestPlayerId = playerId;
        }

        if (nearestPlayerId == -1) return;
        
        Player targetPlayer = Player.Players[nearestPlayerId];
        Collider targetPlayerCollider =
            new(targetPlayer.X, targetPlayer.Y, targetPlayer.Size, targetPlayer.Size);
        quadtree.Insert(targetPlayerCollider);
        
        float moveX = targetPlayer.X - X;
        float moveY = targetPlayer.Y - Y;

        if (moveX != 0f || moveY != 0f)
        {
            float moveMag = MathF.Sqrt(moveX * moveX + moveY * moveY);
            moveX /= moveMag;
            moveY /= moveMag;
        }

        bool hitTargetPlayer = false;
        
        float initialX = X;
        float initialY = Y;
        Collider oldEnemyCollider = new(X, Y, Size, Size);

        UpdateLocal(moveX * Speed, 0, tickTime);
        
        returnColliders.Clear();
        Collider enemyCollider = new(X, Y, Size, Size);
        quadtree.Retrieve(ref returnColliders, enemyCollider);

        foreach (Collider returnCollider in returnColliders)
        {
            if (oldEnemyCollider.Equals(returnCollider) || !enemyCollider.CollidesWith(returnCollider)) continue;
            if (targetPlayerCollider.Equals(returnCollider)) hitTargetPlayer = true;
            
            X = initialX;
            break;
        }
        
        UpdateLocal(0, moveY * Speed, tickTime);
        
        returnColliders.Clear();
        enemyCollider = new(X, Y, Size, Size);
        quadtree.Retrieve(ref returnColliders, enemyCollider);

        foreach (Collider returnCollider in returnColliders)
        {
            if (oldEnemyCollider.Equals(returnCollider) || !enemyCollider.CollidesWith(returnCollider)) continue;
            if (targetPlayerCollider.Equals(returnCollider)) hitTargetPlayer = true;

            Y = initialY;
            break;
        }

        attackTimer -= tickTime;
        
        if (hitTargetPlayer && attackTimer <= 0f)
        {
            AttackPlayer(targetPlayer);
        }
    }

    private void AttackPlayer(Player targetPlayer)
    {
        attackTimer = AttackDelay;
        targetPlayer.Health -= Damage;

        Server.SendMessageToAll(Message.MessageType.PlayerDamage, new PlayerDamageData(targetPlayer.Id, Damage));
    }
    
    public static void UpdateAllRemote(float frameTime)
    {
        foreach ((int _, Enemy enemy) in Enemies)
        {
            enemy.UpdateRemote(frameTime);
        }
    }

    public static int AddEnemy(Enemy enemy)
    {
        int id = NextId++;
        AddEnemyWithId(id, enemy);
        return id;
    }
    
    public static void AddEnemyWithId(int id, Enemy enemy)
    {
        enemy.Id = id;
        Enemies.Add(id, enemy);
    }
    
    public static void ServerAddEnemy(Enemy enemy)
    {
        int newEnemyId = AddEnemy(enemy);
        Server.SendMessageToAll(Message.MessageType.EnemySpawn,
            new EnemySpawnData(newEnemyId, enemy.X, enemy.Y, (int)enemy.Type, enemy.Damage, enemy.Speed, enemy.Size));
    }

    public static void HandleEnemySpawn(Data data)
    {
        if (data is not EnemySpawnData enemySpawnData) return;

        AddEnemyWithId(enemySpawnData.Id,
            NewEnemy((EnemyType)enemySpawnData.Type, enemySpawnData.X, enemySpawnData.Y,
                enemySpawnData.Damage,
                enemySpawnData.Speed,
                enemySpawnData.Size));
    }
    
    public static void HandleEnemyMove(Data data)
    {
        if (data is not EnemyMoveData enemyMoveData) return;
        if (!Enemies.ContainsKey(enemyMoveData.Id)) return;
        
        Enemy targetEnemy = Enemies[enemyMoveData.Id];
        targetEnemy.X = enemyMoveData.X;
        targetEnemy.Y = enemyMoveData.Y;
    }
}