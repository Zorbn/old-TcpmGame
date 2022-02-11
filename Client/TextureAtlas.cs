using Raylib_cs;

namespace Client;

public class TextureAtlas
{
    public Texture2D Texture { get; }
    private int cellWidth, cellHeight;

    public TextureAtlas(Texture2D texture, int cellWidth, int cellHeight)
    {
        Texture = texture;
        this.cellWidth = cellWidth;
        this.cellHeight = cellHeight;
    }

    public Rectangle GetTextureRect(int index)
    {
        int verticalCells = Texture.height / cellHeight;
        
        return new Rectangle
        {
            x = index / verticalCells * cellWidth,
            y = index % verticalCells * cellHeight,
            width = cellWidth,
            height = cellHeight
        };

    }
}