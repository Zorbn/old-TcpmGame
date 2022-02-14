namespace Shared;

public class SyncedEntity
{
    public static float FlashSpeed = 1f;
    public static int NextEntityId;
    
    public int ClientId; // 0+ represents a message stream between the server and player, -1 represents the server
    public float X, Y;
    public float VisualX, VisualY;
    public float FlashAmount;
    public Direction Direction;

    public SyncedEntity(int clientId, float x, float y, Direction direction = Direction.Down)
    {
        ClientId = clientId;
        X = x;
        Y = y;
        VisualX = x;
        VisualY = y;
        Direction = direction;
    }

    public virtual void Destroy()
    {
    }

    public virtual void UpdateLocal(float moveX, float moveY, float frameTime)
    {
        X += moveX * frameTime;
        Y += moveY * frameTime;

        VisualX = X;
        VisualY = Y;

        Direction = DirectionUtils.GetDirection(moveX, moveY, Direction);
        UpdateFx(frameTime);
    }
    
    public virtual void UpdateRemote(float frameTime)
    {
        VisualX = MathUtils.Lerp(VisualX, X, frameTime * 10f);
        VisualY = MathUtils.Lerp(VisualY, Y, frameTime * 10f);

        UpdateFx(frameTime);
    }

    private void UpdateFx(float frameTime)
    {
        FlashAmount -= frameTime * FlashSpeed;
        if (FlashAmount < 0f) FlashAmount = 0f;
    }
}