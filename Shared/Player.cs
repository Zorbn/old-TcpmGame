using Messaging;
using Shared.Items;

namespace Shared;

public class Player : SyncedEntity
{
    public static Dictionary<int, Player> Players = new();

    public int Id;
    public float Speed;
    public int Size;
    public int Health;
    public int MaxHealth;
    public Inventory Inventory;
    
    public const float DefaultSpeed = 150f;

    public Player(int id, float x, float y, int health = 100, int maxHealth = 100, float speed = DefaultSpeed, 
        int size = 32, List<Item.ItemType>? itemTypes = null, Direction direction = Direction.Down) : base(x, y, direction)
    {
        Id = id;
        Speed = speed;
        Size = size;
        MaxHealth = maxHealth;
        Health = health;
        Inventory = new  Inventory();

        if (itemTypes == null)
        {
            Inventory.TryAddItem(Item.NewItem(Item.ItemType.Dagger));
            Inventory.TryAddItem(Item.NewItem(Item.ItemType.Shield));
            Inventory.TryAddItem(Item.NewItem(Item.ItemType.Dagger));
        }
        else
        {
            foreach (Item.ItemType itemType in itemTypes)
            {
                Inventory.TryAddItem(Item.NewItem(itemType));
            }
        }
    }

    public void Destroy()
    {
        Players.Remove(Id);
    }

    public static void UpdateAllRemote(int localId, float frameTime)
    {
        foreach ((int id, Player player) in Players)
        {
            if (id != localId)
            {
                player.UpdateRemote(frameTime);
            }
        }
    }

    public static void ServerHandlePlayerMove(Data data)
    {
        if (data is not PlayerMoveData playerMoveData) return;
        if (!Players.ContainsKey(playerMoveData.Id)) return;

        Player targetPlayer = Players[playerMoveData.Id];
        targetPlayer.X = playerMoveData.X;
        targetPlayer.Y = playerMoveData.Y;

        Server.SendMessageToAllExcluding(playerMoveData.Id, Message.MessageType.PlayerMove, playerMoveData);
    }
    
    public static void HandlePlayerMove(Data data)
    {
        if (data is not PlayerMoveData playerMoveData) return;

        Player targetPlayer = Players[playerMoveData.Id];
        targetPlayer.X = playerMoveData.X;
        targetPlayer.Y = playerMoveData.Y;
    }

    public static void HandlePlayerDamage(Data data)
    {
        if (data is not PlayerDamageData playerDamageData) return;

        Player targetPlayer = Players[playerDamageData.Id];
        targetPlayer.Health -= playerDamageData.Damage;
        targetPlayer.FlashAmount = 1f;
    }
}