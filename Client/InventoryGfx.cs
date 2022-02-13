using System.Numerics;
using Shared;
using Raylib_cs;

namespace Client;

public static class InventoryGfx
{
    public const int SlotSize = 40;
    public const int SlotPadding = 5;
    public const int SlotBorderSize = 2;
    
    private struct SlotRects
    {
        public Rectangle[] OuterSlotRects;
        public Rectangle[] InnerSlotRects;

        public SlotRects(Rectangle[] outerSlotRects, Rectangle[] innerSlotRects)
        {
            OuterSlotRects = outerSlotRects;
            InnerSlotRects = innerSlotRects;
        }
    }
    
    public static void DrawInventory(Inventory inventory, TextureAtlas atlas, int screenWidth, int screenHeight, float zoom)
    {
        int itemCount = inventory.Items.Count;
        SlotRects slotRects = GetSlotRects(screenWidth, screenHeight, zoom);
        
        for (int i = 0; i < Inventory.Capacity; i++)
        {
            Raylib.DrawRectangleRec(slotRects.OuterSlotRects[i], Color.BLACK);
            Raylib.DrawRectangleRec(slotRects.InnerSlotRects[i], Color.WHITE);

            if (i < itemCount)
            {
                SpriteShader.BeginSpriteShaderMode(atlas.Texture.width, atlas.Texture.height, 0f);
                
                Raylib.DrawTexturePro(atlas.Texture, atlas.GetTextureRect(inventory.Items[i].TextureIndex),
                    slotRects.InnerSlotRects[i],
                    Vector2.Zero, 0f, Color.WHITE);
                
                Raylib.EndShaderMode();
            }
        }
    }

    public static void UpdateInventory(int localId, int screenWidth, int screenHeight, float zoom)
    {
        Inventory inventory = Player.Players[localId].Inventory;
        SlotRects slotRects = GetSlotRects(screenWidth, screenHeight, zoom);
        Vector2 mousePos = Raylib.GetMousePosition();
        
        if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
        {
            for (int i = 0; i < inventory.Items.Count; i++)
            {
                Rectangle innerSlotRect = slotRects.InnerSlotRects[i];
                if (Raylib.CheckCollisionPointRec(mousePos, innerSlotRect))
                {
                    Inventory.NetTryDropItem(localId, i);
                    break;
                }
            }
        }
    }

    private static SlotRects GetSlotRects(int screenWidth, int screenHeight, float zoom)
    {
        float scaledSlotSize = SlotSize * zoom;
        float scaledSlotPadding = SlotPadding * zoom;
        float scaledSlotBorderSize = SlotBorderSize * zoom;
        
        float centerX = screenWidth / 2f;
        float centerY = screenHeight - scaledSlotSize / 2 - scaledSlotPadding;
        
        float paddedSlotSide = scaledSlotSize + scaledSlotPadding;
        float startX = centerX - Inventory.Capacity / 2f * paddedSlotSide;

        if (Inventory.Capacity % 2 == 0)
        {
            startX += paddedSlotSide / 2;
        }

        Rectangle[] outerSlotRects = new Rectangle[Inventory.Capacity];
        Rectangle[] innerSlotRects = new Rectangle[Inventory.Capacity];

        for (int i = 0; i < Inventory.Capacity; i++)
        {
            Rectangle outerSlotRect = new()
            {
                x = startX + i * paddedSlotSide - paddedSlotSide / 2,
                y = centerY - scaledSlotSize / 2f,
                width = scaledSlotSize,
                height = scaledSlotSize
            };

            outerSlotRects[i] = outerSlotRect;
            
            Rectangle innerSlotRect = outerSlotRect;
            innerSlotRect.x += scaledSlotBorderSize;
            innerSlotRect.y += scaledSlotBorderSize;
            innerSlotRect.width -= scaledSlotBorderSize * 2f;
            innerSlotRect.height -= scaledSlotBorderSize * 2f;

            innerSlotRects[i] = innerSlotRect;
        }

        return new SlotRects(outerSlotRects, innerSlotRects);
    }
}