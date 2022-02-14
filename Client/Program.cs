using System.Numerics;
using Messaging;
using Raylib_cs;
using Shared;
using Shared.Items;
using Shared.Projectiles;

namespace Client;

internal static class Program
{
    private const int InitialScreenWidth = 800;
    private const int InitialScreenHeight = 450;

    private static readonly Dictionary<Message.MessageType, MessageStream.MessageHandler> MessageHandlers = new()
    {
        { Message.MessageType.ExampleNotification, ExampleNotification.HandleNotification },
        { Message.MessageType.PlayerJoin, HandlePlayerJoin },
        { Message.MessageType.PlayerDisconnect, HandlePlayerDisconnect },
        { Message.MessageType.PlayerMove, Player.HandlePlayerMove },
        { Message.MessageType.PlayerDamage, Player.HandlePlayerDamage },
        { Message.MessageType.EnemySpawn, Enemy.HandleEnemySpawn },
        { Message.MessageType.EnemyMove, Enemy.HandleEnemyMove },
        { Message.MessageType.PlayerDropItem, Inventory.HandlePlayerDropItem },
        { Message.MessageType.UpdateDroppedItems, DroppedItem.HandleUpdateDroppedItems },
        { Message.MessageType.UpdateItem, Inventory.HandleUpdateItem },
        { Message.MessageType.EnemyDamage, Enemy.HandleEnemyDamage }
    };

    private static Camera2D Camera;
    
    private static List<Sprite> DrawList = new();
    private static readonly Quadtree Quadtree = new(0, new Collider(0, 0, 640, 480, null));
    
    private static Texture2D PlayerTexture;
    private static TextureAtlas? ItemAtlas;
    private static TextureAtlas? ProjectileAtlas;
    
    public static void Main()
    {
        Messaging.Client.StartClient("127.0.0.1", MessageHandlers, 60, OnTick, OnDisconnect, OnConnect,
            OnConnectFailed);

        Raylib.SetWindowState(ConfigFlags.FLAG_WINDOW_RESIZABLE);
        Raylib.InitWindow(InitialScreenWidth, InitialScreenHeight, "GameClient");
        Raylib.SetTargetFPS(144);

        Camera = new Camera2D
        {
            offset = Vector2.Zero,
            rotation = 0f,
            target = Vector2.Zero,
            zoom = 1f
        }; 
        
        PlayerTexture = Raylib.LoadTexture("Resources/Player.png");
        
        SpriteShader.LoadSpriteShader();
        
        
        ItemAtlas = new TextureAtlas(Raylib.LoadTexture("Resources/ItemAtlas.png"), 16, 16);
        ProjectileAtlas = new TextureAtlas(Raylib.LoadTexture("Resources/ProjectileAtlas.png"), 16, 16);

        while (!Raylib.WindowShouldClose())
        {
            Update();
        }
        
        Raylib.CloseWindow();
    }

    public static void Update()
    {
        if (ItemAtlas == null) throw new Exception("Item atlas isn't loaded, can't update game state!");
        if (ProjectileAtlas == null) throw new Exception("Projectile atlas isn't loaded, can't update game state!");
        
        int localId = Messaging.Client.GetId();
        float frameTime = Raylib.GetFrameTime();
        DrawList.Clear();
        Quadtree.Clear();

        int screenWidth = Raylib.GetScreenWidth();
        int screenHeight = Raylib.GetScreenHeight();

        if (localId != -1)
        {
            Player.UpdateAllRemote(localId, frameTime);
            Inventory.UpdateAllItemsLocal(localId, frameTime);
            InventoryGfx.UpdateInventory(localId, screenWidth, screenHeight, Camera.zoom);
        }

        Enemy.UpdateAllRemote(frameTime);
        
        foreach ((int id, Player player) in Player.Players.ToArray())
        {
            if (localId != -1 && id == localId)
            {
                float moveX = 0f;
                float moveY = 0f;

                if (Raylib.IsKeyDown(KeyboardKey.KEY_A)) moveX -= 1;
                if (Raylib.IsKeyDown(KeyboardKey.KEY_D)) moveX += 1;
                if (Raylib.IsKeyDown(KeyboardKey.KEY_W)) moveY -= 1;
                if (Raylib.IsKeyDown(KeyboardKey.KEY_S)) moveY += 1;

                if (moveX != 0f || moveY != 0f)
                {
                    float moveMag = MathF.Sqrt(moveX * moveX + moveY * moveY);
                    moveX /= moveMag;
                    moveY /= moveMag;
                }
                
                player.UpdateLocal(moveX * player.Speed, moveY * player.Speed, frameTime);
                
                Player localPlayer = Player.Players[localId];

                Camera.target = new Vector2(
                    (int)localPlayer.VisualX -
                    screenWidth / Camera.zoom / 2f,
                    (int)localPlayer.VisualY -
                    screenHeight / Camera.zoom / 2f);

                Camera.zoom = (float)screenHeight / InitialScreenHeight;
            }

            DrawList.Add(new Sprite(PlayerTexture, new Rectangle(0f, 0f, 16f, 16f), new Rectangle(
                    (int)player.VisualX,
                    (int)player.VisualY, player.Size, player.Size), player.VisualY,
                0f, player.FlashAmount));
        }

        // Many of these lists may get modified during the update, so standard for or ToArray is used
        foreach ((int _, Enemy enemy) in Enemy.Enemies.ToArray())
        {
            DrawList.Add(new Sprite(PlayerTexture, new Rectangle(0f, 0f, 16f, 16f), new Rectangle(
                (int)enemy.VisualX,
                (int)enemy.VisualY,
                enemy.Size, enemy.Size), enemy.VisualY, 0f, enemy.FlashAmount));

            Quadtree.Insert(new Collider(enemy.X, enemy.Y, enemy.Size,
                enemy.Size, enemy));
        }

        for (int index = 0; index < DroppedItem.DroppedItems.Count; index++)
        {
            DroppedItem droppedItem = DroppedItem.DroppedItems[index];
            DrawList.Add(new Sprite(ItemAtlas.Texture, ItemAtlas.GetTextureRect(droppedItem.Item.TextureIndex),
                new Rectangle(droppedItem.X,
                    droppedItem.Y,
                    DroppedItem.Size, DroppedItem.Size), droppedItem.Y));
        }

        for (int index = 0; index < Projectile.Projectiles.Count; index++)
        {
            Projectile projectile = Projectile.Projectiles[index];
            DrawList.Add(new Sprite(ProjectileAtlas.Texture, ProjectileAtlas.GetTextureRect(projectile.TextureIndex),
                new Rectangle(projectile.X + projectile.OffsetX, projectile.Y + projectile.OffsetY, Projectile.Size,
                    Projectile.Size), projectile.Y, projectile.Rotation));
        }

        Projectile.UpdateAll(frameTime, Quadtree, false);

        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.BEIGE);
        
        Raylib.BeginMode2D(Camera);

        DrawList.Sort((first, second) => first.YIndex.CompareTo(second.YIndex));

        foreach (Sprite sprite in DrawList)
        {
            SpriteShader.BeginSpriteShaderMode(sprite.Texture.width, sprite.Texture.height, sprite.FlashAmount);

            Raylib.DrawTexturePro(sprite.Texture, sprite.Source, sprite.Destination,
                new Vector2(sprite.Destination.width / 2f, sprite.Destination.height / 2f), sprite.Rotation,
                Color.WHITE);
            
            Raylib.EndShaderMode();
        }

        Raylib.EndMode2D();
        
        if (localId != -1)
        {
            InventoryGfx.DrawInventory(Player.Players[localId].Inventory, ItemAtlas, screenWidth,
                screenHeight, Camera.zoom);
        }
        
        Raylib.DrawFPS(10, 10);

        Raylib.EndDrawing();
    }

    public static void OnTick()
    {
        int localId = Messaging.Client.GetId();

        Player localPlayer = Player.Players[localId];

        Messaging.Client.SendMessage(Message.MessageType.PlayerMove,
            new PlayerMoveData(localPlayer.Id, localPlayer.X, localPlayer.Y));
    }

    public static void OnDisconnect(int id)
    {
        Console.WriteLine("Disconnected!");
    }

    public static void OnConnect()
    {
        Console.WriteLine("Connected!");
    }

    public static void OnConnectFailed()
    {
        Console.WriteLine("Connection failed!");
    }

    private static void HandlePlayerJoin(Data data)
    {
        if (data is not PlayerJoinData playerJoinData) return;

        Player.Players.Add(playerJoinData.Id,
            new Player(playerJoinData.Id, playerJoinData.X, playerJoinData.Y, playerJoinData.Health,
                playerJoinData.MaxHealth, playerJoinData.Speed, playerJoinData.Size,
                playerJoinData.ItemTypes.Cast<Item.ItemType>().ToList()));
    }
    
    private static void HandlePlayerDisconnect(Data data)
    {
        if (data is not PlayerDisconnectData playerDisconnectData) return;
        if (!Player.Players.ContainsKey(playerDisconnectData.Id)) return;
        
        Player.Players[playerDisconnectData.Id].Destroy();
    }
}