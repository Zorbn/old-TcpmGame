using Raylib_cs;

namespace Client;

public class TextureAtlas
{
    public Texture2D Texture { get; }
    public int CellWidth { get; }
    public int CellHeight { get; }

    public TextureAtlas(Texture2D texture, int cellWidth, int cellHeight)
    {
        Texture = texture;
        CellWidth = cellWidth;
        CellHeight = cellHeight;
    }

    public Rectangle GetTextureRect(int index)
    {
        int verticalCells = Texture.height / CellHeight;
        
        return new Rectangle
        {
            x = index / verticalCells * CellWidth,
            y = index % verticalCells * CellHeight,
            width = CellWidth,
            height = CellHeight
        };

    }
}