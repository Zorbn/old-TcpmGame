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

    protected Item(int textureIndex, ItemType type)
    {
        TextureIndex = textureIndex;
        Type = type;
    }

    public void Update(float frameTime)
    {
        
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