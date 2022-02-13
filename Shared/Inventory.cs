using Messaging;
using Shared.Items;

namespace Shared;

public class Inventory
{
    public static int Capacity = 10;

    public List<Item> Items { get; }

    public Inventory()
    {
        Items = new List<Item>();
    }

    public bool TryAddItem(Item item)
    {
        if (Items.Count >= Capacity) return false;

        Items.Add(item);
        return true;
    }

    public Item? TryRemoveItem(int index)
    {
        if (index >= Items.Count) return null;

        Item droppedItem = Items[index];
        Items.RemoveAt(index);

        return droppedItem;
    }

    public static void UpdateAllItemsLocal(int localId, float frameTime)
    {
        Inventory inventory = Player.Players[localId].Inventory;
        
        for (int index = 0; index < inventory.Items.Count; index++)
        {
            Item item = inventory.Items[index];
            if (!item.CanUpdate(frameTime)) continue;

            item.Update(localId);
            Client.SendMessage(Message.MessageType.UpdateItem, new UpdateItemData(localId, index));
        }
    }

    public static void TryDropItem(int localId, int index)
    {
        Player localPlayer = Player.Players[localId];
        Item? removedItem = localPlayer.Inventory.TryRemoveItem(index);
        if (removedItem != null)
        {
            DroppedItem.DropItem(removedItem, localPlayer.X, localPlayer.Y);
        }
    }
    
    public static void NetTryDropItem(int localId, int index)
    {
        Client.SendMessage(Message.MessageType.PlayerDropItem, new PlayerDropItemData(localId, index));
    }

    public static void HandlePlayerDropItem(Data data)
    {
        if (data is not PlayerDropItemData playerDropItemData) return;

        TryDropItem(playerDropItemData.Id, playerDropItemData.Index);
    }

    public static void ServerHandlePlayerDropItem(Data data)
    {
        if (data is not PlayerDropItemData playerDropItemData) return;
        
        TryDropItem(playerDropItemData.Id, playerDropItemData.Index);
        Server.SendMessageToAll(Message.MessageType.PlayerDropItem, playerDropItemData);
    }

    public static void HandleUpdateItem(Data data)
    {
        if (data is not UpdateItemData updateItemData) return;
        
        Player.Players[updateItemData.PlayerId].Inventory.Items[updateItemData.Index].Update(updateItemData.PlayerId);
    }

    public static void ServerHandleUpdateItem(Data data)
    {
        if (data is not UpdateItemData updateItemData) return;
        
        Player.Players[updateItemData.PlayerId].Inventory.Items[updateItemData.Index].Update(updateItemData.PlayerId);
        
        Server.SendMessageToAllExcluding(updateItemData.PlayerId, Message.MessageType.UpdateItem, data);
    }
}