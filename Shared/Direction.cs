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
}