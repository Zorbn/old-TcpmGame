using Messaging;
using Shared.Items;

namespace Shared;

public class DroppedItem
{
    public static List<DroppedItem> DroppedItems = new();
    public const int Size = 32;
    
    public float X { get; }
    public float Y { get; }

    public Item Item { get; }

    private DroppedItem(Item item, float x, float y)
    {
        Item = item;
        X = x;
        Y = y;
    }

    public static void DropItem(Item item, float x, float y)
    {
        DroppedItems.Add(new DroppedItem(item, x, y));
    }
    
    public static void DropItem(Item.ItemType itemType, float x, float y)
    {
        DroppedItems.Add(new DroppedItem(Item.NewItem(itemType), x, y));
    }

    public static UpdateDroppedItemsData MakeUpdateDroppedItemsData()
    {
        List<int> droppedItemTypes = new();
        List<float> droppedItemXs = new();
        List<float> droppedItemYs = new();
        
        foreach (DroppedItem droppedItem in DroppedItems)
        {
            droppedItemTypes.Add((int)droppedItem.Item.Type);
            droppedItemXs.Add(droppedItem.X);
            droppedItemYs.Add(droppedItem.Y);
        }
        
        return new UpdateDroppedItemsData(droppedItemTypes, droppedItemXs, droppedItemYs);
    }

    public static void HandleUpdateDroppedItems(Data data)
    {
        if (data is not UpdateDroppedItemsData updateData) return;
        
        DroppedItems.Clear();

        for (int i = 0; i < updateData.DroppedItemTypes.Count; i++)
        {
            DropItem((Item.ItemType)updateData.DroppedItemTypes[i], updateData.DroppedItemXs[i],
                updateData.DroppedItemYs[i]);
        }
    }
}