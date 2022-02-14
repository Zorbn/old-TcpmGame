using Messaging;
using Shared;
using Shared.Items;
using Shared.Projectiles;

namespace Server;

internal static class Program
{
    private static readonly Dictionary<Message.MessageType, MessageStream.MessageHandler>? MessageHandlers = new()
    {
        { Message.MessageType.ExampleNotification, ExampleNotification.HandleNotification },
        { Message.MessageType.PlayerMove, Player.ServerHandlePlayerMove },
        { Message.MessageType.PlayerDropItem, Inventory.ServerHandlePlayerDropItem },
        { Message.MessageType.UpdateItem, Inventory.ServerHandleUpdateItem }
    };

    private static readonly Random Rng = new();
    private static readonly Quadtree Quadtree = new(0, new Collider(0, 0, 640, 480, null));

    private static bool spawnedEnemies;

    public static void Main()
    {
        Messaging.Server.StartServer("127.0.0.1", MessageHandlers, 60, OnTick, OnDisconnect, OnClientConnect);
    }
    
    private static void OnTick()
    {
        float tickTime = 1f / Messaging.Server.TickRate;

        Player.UpdateAllRemote(-1, tickTime);
        
        /*
        if (Enemy.Enemies.Count < 3)
        {
            Enemy.ServerAddEnemy(Enemy.NewEnemy(Enemy.EnemyType.Default, Rng.Next(100, 300), Rng.Next(100, 300), 10));
        }
        */

        if (!spawnedEnemies)
        {
            spawnedEnemies = true;
            Enemy.ServerAddEnemy(Enemy.NewEnemy(Enemy.EnemyType.Default, Rng.Next(100, 300), Rng.Next(100, 300), 10));
            Enemy.ServerAddEnemy(Enemy.NewEnemy(Enemy.EnemyType.Default, Rng.Next(100, 300), Rng.Next(100, 300), 10));
            Enemy.ServerAddEnemy(Enemy.NewEnemy(Enemy.EnemyType.Default, Rng.Next(100, 300), Rng.Next(100, 300), 10));
        }
        
        Quadtree.Clear();

        foreach ((int _, Enemy enemy) in Enemy.Enemies)
        {
            Quadtree.Insert(new Collider(enemy.X, enemy.Y, enemy.Size, enemy.Size, enemy));
        }
        
        Projectile.UpdateAll(tickTime, Quadtree, true);

        List<Collider> returnColliders = new();
        foreach ((int id, Enemy enemy) in Enemy.Enemies)
        {
            enemy.ChaseNearestPlayer(tickTime, Quadtree, returnColliders);

            Messaging.Server.SendMessageToAll(Message.MessageType.EnemyMove, new EnemyMoveData(id, enemy.X, enemy.Y));
        }
    }

    private static void OnClientConnect(int id)
    {
        Player.Players.Add(id, new Player(id, Rng.Next(0, 100), Rng.Next(0, 100)));

        foreach ((int playerId, Player player) in Player.Players)
        {
            PlayerJoinData playerJoinData = new(playerId, player.X, player.Y, player.Health, player.MaxHealth,
                player.Speed, player.Size, Item.GetItemTypes(player.Inventory.Items).Cast<int>().ToList());

            if (playerId != id)
            {
                Messaging.Server.SendMessage(id, Message.MessageType.PlayerJoin, playerJoinData);
            }
            else
            {
                Messaging.Server.SendMessageToAll(Message.MessageType.PlayerJoin, playerJoinData);
            }
        }

        foreach ((int enemyId, Enemy enemy) in Enemy.Enemies)
        {
            EnemySpawnData enemySpawnData = new(enemyId, enemy.X, enemy.Y, (int)enemy.Type, enemy.Health,
                enemy.MaxHealth, enemy.Damage, enemy.Speed,
                enemy.Size);

            Messaging.Server.SendMessage(id, Message.MessageType.EnemySpawn, enemySpawnData);
        }

        Messaging.Server.SendMessage(id, Message.MessageType.UpdateDroppedItems,
            DroppedItem.MakeUpdateDroppedItemsData());
    }

    private static void OnDisconnect(int id)
    {
        Player.Players[id].Destroy();
        Messaging.Server.SendMessageToAll(Message.MessageType.PlayerDisconnect, new PlayerDisconnectData(id));
    }
}