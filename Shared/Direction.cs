using System.Numerics;

namespace Shared;

public enum Direction
{
    Up,
    Down,
    Left,
    Right
}

public static class DirectionUtils
{
    public static Vector2 ToVector(Direction direction) => direction switch
    {
        Direction.Up => new Vector2(0f, -1f),
        Direction.Down => new Vector2(0f, 1f),
        Direction.Left => new Vector2(-1f, 0f),
        Direction.Right => new Vector2(1f, 0f),
        _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
    };
    
    public static Direction GetDirection(float moveX, float moveY, Direction initialDirection = Direction.Down)
    {
        Direction direction = initialDirection;
        
        if (MathF.Abs(moveX) > MathF.Abs(moveY))
        {
            if (moveX > 0)
            {
                direction = Direction.Right;
            }
            else if (moveX < 0)
            {
                direction = Direction.Left;
            }
        }
        else
        {
            if (moveY > 0)
            {
                direction = Direction.Down;
            }
            else if (moveY < 0)
            {
                direction = Direction.Up;
            }   
        }

        return direction;
    }
}