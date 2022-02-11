using System.Numerics;
using Messaging;
using Raylib_cs;
using Shared;
using Shared.Items;

namespace Client;

internal static class Program
{
    private const int InitialScreenWidth = 800;
    private const int InitialScreenHeight = 450;
    
    private static readonly Dictionary<Message.MessageType, MessageStream.MessageHandler>? MessageHandlers = new()
    {
        { Message.MessageType.ExampleNotification, ExampleNotification.HandleNotification },
        { Message.MessageType.PlayerJoin, HandlePlayerJoin },
        { Message.MessageType.PlayerDisconnect, HandlePlayerDisconnect },
        { Message.MessageType.PlayerMove, Player.HandlePlayerMove },
        { Message.MessageType.PlayerDamage, Player.HandlePlayerDamage },
        { Message.MessageType.EnemySpawn, Enemy.HandleEnemySpawn },
        { Message.MessageType.EnemyMove, Enemy.HandleEnemyMove },
        { Message.MessageType.PlayerDropItem, Inventory.HandlePlayerDropItem },
        { Message.MessageType.UpdateDroppedItems, DroppedItem.HandleUpdateDroppedItems }
    };

    private static Camera2D Camera;
    
    private static List<Sprite> DrawList = new();
    private static Shader SpriteShader;
    private static Texture2D PlayerTexture;
    private static int SpriteShaderFlashAmountLoc;
    private static int TextureSizeLoc;
    private static TextureAtlas? ItemAtlas;

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
        
        LoadSpriteShader();
        SpriteShaderFlashAmountLoc = Raylib.GetShaderLocation(SpriteShader, "flashAmount");
        TextureSizeLoc = Raylib.GetShaderLocation(SpriteShader, "textureSize");
        
        ItemAtlas = new TextureAtlas(Raylib.LoadTexture("Resources/ItemAtlas.png"), 16, 16);

        while (!Raylib.WindowShouldClose())
        {
            Update();
        }
        
        Raylib.CloseWindow();
    }

    public static void Update()
    {
        int localId = Messaging.Client.GetId();
        float frameTime = Raylib.GetFrameTime();
        DrawList.Clear();

        int screenWidth = Raylib.GetScreenWidth();
        int screenHeight = Raylib.GetScreenHeight();

        if (localId != -1)
        {
            Player.UpdateAllRemote(localId, frameTime);
            InventoryGfx.UpdateInventory(localId, screenWidth, screenHeight, Camera.zoom);
        }
        
        Enemy.UpdateAllRemote(frameTime);

        foreach ((int id, Player player) in Player.Players)
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
                    (int)(localPlayer.VisualX - player.Size / 2f) + player.Size / 2f - screenWidth / Camera.zoom / 2f,
                    (int)(localPlayer.VisualY - player.Size / 2f) + player.Size / 2f - screenHeight / Camera.zoom / 2f);

                Camera.zoom = (float)screenHeight / InitialScreenHeight;
            }

            DrawList.Add(new Sprite(PlayerTexture, new Rectangle(0f, 0f, 16f, 16f), new Rectangle(
                    (int)(player.VisualX - player.Size / 2f),
                    (int)(player.VisualY - player.Size / 2f), player.Size, player.Size), player.VisualY,
                player.FlashAmount));
        }

        foreach ((int _, Enemy enemy) in Enemy.Enemies)
        {
            DrawList.Add(new Sprite(PlayerTexture, new Rectangle(0f, 0f, 16f, 16f), new Rectangle(
                (int)(enemy.VisualX - enemy.Size / 2f),
                (int)(enemy.VisualY - enemy.Size / 2f),
                enemy.Size, enemy.Size), enemy.VisualY, enemy.FlashAmount));
        }

        foreach (DroppedItem droppedItem in DroppedItem.DroppedItems)
        {
            DrawList.Add(new Sprite(ItemAtlas!.Texture, ItemAtlas.GetTextureRect(droppedItem.Item.TextureIndex),
                new Rectangle(droppedItem.X - DroppedItem.Size / 2f,
                    droppedItem.Y - DroppedItem.Size / 2f,
                    DroppedItem.Size, DroppedItem.Size), droppedItem.Y, 0f));
        }

        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.BEIGE);
        
        Raylib.BeginMode2D(Camera);

        DrawList.Sort((first, second) => first.YIndex.CompareTo(second.YIndex));
        
        
        foreach (Sprite sprite in DrawList)
        {
            Raylib.SetShaderValue(SpriteShader, SpriteShaderFlashAmountLoc, sprite.FlashAmount,
                ShaderUniformDataType.SHADER_UNIFORM_FLOAT);
            Raylib.SetShaderValue(SpriteShader, TextureSizeLoc,
                new float[] { sprite.Texture.width, sprite.Texture.height },
                ShaderUniformDataType.SHADER_UNIFORM_VEC2);
            
            Raylib.BeginShaderMode(SpriteShader);

            Raylib.DrawTexturePro(sprite.Texture, sprite.Source, sprite.Destination, Vector2.One, 0f, Color.WHITE);
            
            Raylib.EndShaderMode();
        }
        
        Raylib.EndMode2D();
        
        if (localId != -1)
        {
            InventoryGfx.DrawInventory(Player.Players[localId].Inventory, ItemAtlas, screenWidth,
                screenHeight, Camera.zoom);
        }

        Raylib.EndDrawing();
    }

    public static void OnTick()
    {
        int localId = Messaging.Client.GetId();

        Player localPlayer = Player.Players[localId];

        Messaging.Client.SendMessage(Message.MessageType.PlayerMove,
            new PlayerMoveData(localPlayer.Id, localPlayer.X, localPlayer.Y));
    }

    private static void LoadSpriteShader()
    {
        SpriteShader = Raylib.LoadShader(null, "Resources/SpriteShader.frag");

        float outlineSize = 1f;
        float[] outlineColor = { 0f, 0f, 0f, 1f };

        int outlineSizeLoc = Raylib.GetShaderLocation(SpriteShader, "outlineSize");
        int outlineColorLoc = Raylib.GetShaderLocation(SpriteShader, "outlineColor");

        Raylib.SetShaderValue(SpriteShader, outlineSizeLoc, outlineSize, ShaderUniformDataType.SHADER_UNIFORM_FLOAT);
        Raylib.SetShaderValue(SpriteShader, outlineColorLoc, outlineColor, ShaderUniformDataType.SHADER_UNIFORM_VEC4);
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

        Player.Players.Remove(playerDisconnectData.Id);
    }
}