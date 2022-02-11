using Raylib_cs;

namespace Client;

public struct Sprite
{
    public Texture2D Texture;
    public Rectangle Source;
    public Rectangle Destination;
    public float YIndex;
    public float FlashAmount;

    public Sprite(Texture2D texture, Rectangle source, Rectangle destination, float yIndex, float flashAmount)
    {
        Texture = texture;
        Source = source;
        Destination = destination;
        YIndex = yIndex;
        FlashAmount = flashAmount;
    }
}