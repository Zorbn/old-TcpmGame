namespace Shared.Items;

public class Item
{
    public enum ItemType
    {
        Dagger,
        Shield
    }
    
    public int TextureIndex { get; }
    public ItemType Type { get; }
    public float CooldownTime { get; }
    
    private float cooldownTimer;

    protected Item(int textureIndex, ItemType type, float cooldownTime)
    {
        TextureIndex = textureIndex;
        Type = type;
        CooldownTime = cooldownTime;
        cooldownTimer = CooldownTime;
    }

    public virtual void Update(int playerId)
    {
        
    }

    public bool CanUpdate(float frameTime)
    {
        cooldownTimer -= frameTime;

        if (cooldownTimer <= 0f)
        {
            cooldownTimer = CooldownTime;
            return true;
        }

        return false;
    }
    
    public static List<ItemType> GetItemTypes(List<Item> items)
    {
        List<ItemType> itemTypes = new();

        foreach (Item item in items)
        {
            itemTypes.Add(item.Type);
        }

        return itemTypes;
    }
    
    public static Item NewItem(ItemType type)
    {
        return type switch
        {
            ItemType.Dagger => new Dagger(0),
            ItemType.Shield => new Shield(1),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}