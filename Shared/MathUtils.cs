namespace Shared;

public static class MathUtils
{
    public static float Lerp(float initial, float target, float delta)
    {
        return initial += (target - initial) * delta;
    }
}