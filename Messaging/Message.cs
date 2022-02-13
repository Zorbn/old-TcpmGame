using System.Text.Json.Serialization;

namespace Messaging;

// TODO: Make these use constructors, so that every field must be filled
// Make each field into a get only property

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
    [JsonInclude] public int Id { get; }
    [JsonInclude] public float X { get; }
    [JsonInclude] public float Y { get; }
    [JsonInclude] public int Health { get; }
    [JsonInclude] public int MaxHealth { get; }
    [JsonInclude] public float Speed  { get; }
    [JsonInclude] public int Size { get; }
    [JsonInclude] public List<int> ItemTypes { get; }

    public PlayerJoinData(int id, float x, float y, int health, int maxHealth, float speed, int size, List<int> itemTypes)
    {
        Id = id;
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
    [JsonInclude] public int Id { get; }

    public PlayerDisconnectData(int id)
    {
        Id = id;
    }
}

[Serializable]
public class PlayerMoveData : Data
{
    [JsonInclude] public int Id { get; }
    [JsonInclude] public float X { get; }
    [JsonInclude] public float Y { get; }

    public PlayerMoveData(int id, float x, float y)
    {
        Id = id;
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
    [JsonInclude] public int Damage { get; }
    [JsonInclude] public float Speed { get; }
    [JsonInclude] public int Size { get; }

    public EnemySpawnData(int id, float x, float y, int type, int damage, float speed, int size)
    {
        Id = id;
        X = x;
        Y = y;
        Type = type;
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
    [JsonInclude] public int Id { get; }
    [JsonInclude] public int Damage { get; }

    public PlayerDamageData(int id, int damage)
    {
        Id = id;
        Damage = damage;
    }
}

[Serializable]
public class PlayerDropItemData : Data
{
    [JsonInclude] public int Id { get; }
    [JsonInclude] public int Index { get; }

    public PlayerDropItemData(int id, int index)
    {
        Id = id;
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
    [JsonInclude] public int PlayerId;
    [JsonInclude] public int Index;

    public UpdateItemData(int playerId, int index)
    {
        PlayerId = playerId;
        Index = index;
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
        UpdateItem
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