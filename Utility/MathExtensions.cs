using System;
using Godot;

namespace GameCore.Utility;

public static class MathExtensions
{
    public static Vector2 GDExSign(this Vector2 vec)
    {
        var x = vec.X > 0 ? 1 : vec.X < 0 ? -1 : 0;
        var y = vec.Y > 0 ? 1 : vec.Y < 0 ? -1 : 0;
        return new Vector2(x, y);
    }

    public static float MoveToward(this float from, double to, double delta)
    {
        return Mathf.MoveToward(from, (float)to, (float)delta);
    }

    public static float LerpClamp(this float val, double target, double maxMove)
    {
        return val.LerpClamp((float)target, (float)maxMove);
    }

    public static float LerpClamp(this float val, float target, float maxMove)
    {
        return val < target ? Math.Min(val + maxMove, target) : Math.Max(val - maxMove, target);
    }

    public static int MoveTowards(this int current, int target, int delta)
    {
        if (Math.Abs(target - current) <= delta)
            return target;
        return current + Math.Sign(target - current) * delta;
    }

    public static Vector2 SetX(this Vector2 vec, float x) => new(x, vec.Y);

    public static Vector2 SetY(this Vector2 vec, float y) => new(vec.X, y);
}
