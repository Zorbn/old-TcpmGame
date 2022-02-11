namespace Shared;

public class SyncedEntity
{
    public float X, Y;
    public float VisualX, VisualY;
    public float FlashAmount;
    public Direction Direction;

    public SyncedEntity(float x, float y, Direction direction = Direction.Down)
    {
        X = x;
        Y = y;
        VisualX = x;
        VisualY = y;
        Direction = direction;
    }

    public void UpdateLocal(float moveX, float moveY, float frameTime)
    {
        X += moveX * frameTime;
        Y += moveY * frameTime;

        VisualX = X;
        VisualY = Y;

        UpdateDirection(moveX, moveY);
        UpdateFx(frameTime);
    }
    
    public void UpdateRemote(float frameTime)
    {
        UpdateDirection(X - VisualX, Y - VisualY);

        VisualX = MathUtils.Lerp(VisualX, X, frameTime * 10f);
        VisualY = MathUtils.Lerp(VisualY, Y, frameTime * 10f);

        UpdateFx(frameTime);
    }

    private void UpdateFx(float frameTime)
    {
        FlashAmount -= frameTime;
        if (FlashAmount < 0f) FlashAmount = 0f;
    }

    private void UpdateDirection(float moveX, float moveY)
    {
        if (moveY > moveX)
        {
            if (moveY > 0)
            {
                Direction = Direction.Down;
            }
            else if (moveY < 0)
            {
                Direction = Direction.Up;
            }
        }
        else
        {
            if (moveX > 0)
            {
                Direction = Direction.Right;
            }
            else if (moveX < 0)
            {
                Direction = Direction.Left;
            }
        }
    }
}