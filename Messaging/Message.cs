using System.Text.Json.Serialization;

namespace Messaging;

[Serializable]
public class Data {}

[Serializable]
public class InitializeData : Data
{
    [JsonInclude] public int Id { get; }

    public InitializeData(int id)
    {
        Id = id;
    }
}

[Serializable]
public class ExampleNotificationData : Data
{
    [JsonInclude] public string Text { get; }

    public ExampleNotificationData(string text)
    {
        Text = text;
    }
}

[Serializable]
public class PlayerJoinData : Data
{
    [JsonInclude] public int ClientId { get; }
    [JsonInclude] public float X { get; }
    [JsonInclude] public float Y { get; }
    [JsonInclude] public int Health { get; }
    [JsonInclude] public int MaxHealth { get; }
    [JsonInclude] public float Speed  { get; }
    [JsonInclude] public int Size { get; }
    [JsonInclude] public List<int> ItemTypes { get; }

    public PlayerJoinData(int clientId, float x, float y, int health, int maxHealth, float speed, int size, List<int> itemTypes)
    {
        ClientId = clientId;
        X = x;
        Y = y;
        Health = health;
        MaxHealth = maxHealth;
        Speed = speed;
        Size = size;
        ItemTypes = itemTypes;
    }
}

[Serializable]
public class PlayerDisconnectData : Data
{
    [JsonInclude] public int ClientId { get; }

    public PlayerDisconnectData(int clientId)
    {
        ClientId = clientId;
    }
}

[Serializable]
public class PlayerMoveData : Data
{
    [JsonInclude] public int ClientId { get; }
    [JsonInclude] public float X { get; }
    [JsonInclude] public float Y { get; }

    public PlayerMoveData(int clientId, float x, float y)
    {
        ClientId = clientId;
        X = x;
        Y = y;
    }
}

[Serializable]
public class EnemySpawnData : Data
{
    [JsonInclude] public int Id { get; }
    [JsonInclude] public float X { get; }
    [JsonInclude] public float Y { get; }
    [JsonInclude] public int Type { get; }
    [JsonInclude] public int Health { get; }
    [JsonInclude] public int MaxHealth { get; }
    [JsonInclude] public int Damage { get; }
    [JsonInclude] public float Speed { get; }
    [JsonInclude] public int Size { get; }

    public EnemySpawnData(int id, float x, float y, int type, int health, int maxHealth, int damage, float speed, int size)
    {
        Id = id;
        X = x;
        Y = y;
        Type = type;
        Health = health;
        MaxHealth = maxHealth;
        Damage = damage;
        Speed = speed;
        Size = size;
    }
}

[Serializable]
public class EnemyMoveData : Data
{
    [JsonInclude] public int Id { get; }
    [JsonInclude] public float X { get; }
    [JsonInclude] public float Y { get; }

    public EnemyMoveData(int id, float x, float y)
    {
        Id = id;
        X = x;
        Y = y;
    }
}

[Serializable]
public class PlayerDamageData : Data
{
    [JsonInclude] public int ClientId { get; }
    [JsonInclude] public int Damage { get; }

    public PlayerDamageData(int clientId, int damage)
    {
        ClientId = clientId;
        Damage = damage;
    }
}

[Serializable]
public class EnemyDamageData : Data
{
    [JsonInclude] public int Id { get; }
    [JsonInclude] public int Damage { get; }

    public EnemyDamageData(int id, int damage)
    {
        Id = id;
        Damage = damage;
    }
}

[Serializable]
public class PlayerDropItemData : Data
{
    [JsonInclude] public int ClientId { get; }
    [JsonInclude] public int Index { get; }

    public PlayerDropItemData(int clientId, int index)
    {
        ClientId = clientId;
        Index = index;
    }
}

[Serializable]
public class UpdateDroppedItemsData : Data
{
    [JsonInclude] public List<int> DroppedItemTypes { get; }
    [JsonInclude] public List<float> DroppedItemXs { get; }
    [JsonInclude] public List<float> DroppedItemYs { get; }

    public UpdateDroppedItemsData(List<int> droppedItemTypes, List<float> droppedItemXs, List<float> droppedItemYs)
    {
        DroppedItemTypes = droppedItemTypes;
        DroppedItemXs = droppedItemXs;
        DroppedItemYs = droppedItemYs;
    }
}

[Serializable]
public class UpdateItemData : Data
{
    [JsonInclude] public int PlayerClientId { get; }
    [JsonInclude] public int Index { get; }

    public UpdateItemData(int playerClientId, int index)
    {
        PlayerClientId = playerClientId;
        Index = index;
    }
}

[Serializable]
public class PlayerUpdateDirectionData : Data
{
    [JsonInclude] public int ClientId { get; }
    [JsonInclude] public int Direction { get; }

    public PlayerUpdateDirectionData(int clientId, int direction)
    {
        ClientId = clientId;
        Direction = direction;
    }
}

[Serializable]
public class EnemyUpdateDirectionData : Data
{
    [JsonInclude] public int Id { get; }
    [JsonInclude] public int Direction { get; }

    public EnemyUpdateDirectionData(int id, int direction)
    {
        Id = id;
        Direction = direction;
    }
}

public class Message
{
    public enum MessageType
    {
        Initialize,
        ExampleNotification,
        PlayerJoin,
        PlayerDisconnect,
        PlayerMove,
        EnemySpawn,
        EnemyMove,
        PlayerDamage,
        PlayerDropItem,
        UpdateDroppedItems,
        UpdateItem,
        EnemyDamage,
        PlayerUpdateDirection,
        EnemyUpdateDirection
    }
    
    public static Type ToDataType(MessageType messageType) => messageType switch
    {
        MessageType.Initialize => typeof(InitializeData),
        MessageType.ExampleNotification => typeof(ExampleNotificationData),
        MessageType.PlayerJoin => typeof(PlayerJoinData),
        MessageType.PlayerDisconnect => typeof(PlayerDisconnectData),
        MessageType.PlayerMove => typeof(PlayerMoveData),
        MessageType.EnemySpawn => typeof(EnemySpawnData),
        MessageType.EnemyMove => typeof(EnemyMoveData),
        MessageType.PlayerDamage => typeof(PlayerDamageData),
        MessageType.PlayerDropItem => typeof(PlayerDropItemData),
        MessageType.UpdateDroppedItems => typeof(UpdateDroppedItemsData),
        MessageType.UpdateItem => typeof(UpdateItemData),
        MessageType.EnemyDamage => typeof(EnemyDamageData),
        MessageType.PlayerUpdateDirection => typeof(PlayerUpdateDirectionData),
        MessageType.EnemyUpdateDirection => typeof(EnemyUpdateDirectionData),
        _ => throw new ArgumentOutOfRangeException($"No data type corresponds to {messageType}!")
    };

    private readonly MessageType messageType; 
    private readonly Data data;

    public Message(MessageType messageType, Data data)
    {
        this.messageType = messageType;
        this.data = data;
    }

    public byte[] ToByteArray()
    {
        List<byte> bytes = new();
        bytes.AddRange(ByteUtils.ObjectToByteArray(ToDataType(messageType), data));
        bytes.InsertRange(0, BitConverter.GetBytes((int) messageType));
        bytes.InsertRange(0, BitConverter.GetBytes(bytes.Count + sizeof(int))); // Add message length

        return bytes.ToArray();
    }
}