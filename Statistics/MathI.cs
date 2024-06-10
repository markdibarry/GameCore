using System;

namespace GameCore.Statistics;

public static class MathI
{
    public static bool Compare(this CompareOp op, int a, int b)
    {
        return op switch
        {
            CompareOp.Equals => a == b,
            CompareOp.NotEquals => a != b,
            CompareOp.LessEquals => a <= b,
            CompareOp.GreaterEquals => a >= b,
            CompareOp.Less => a < b,
            CompareOp.Greater => a > b,
            CompareOp.None or
            _ => false
        };
    }

    public static float Compute(string op, float a, float b)
    {
        return op switch
        {
            ModOp.Add or
            ModOp.BaseAdd or
            ModOp.PercentAdd => a + b,
            ModOp.PercentMult => a * (1 + b),
            ModOp.Negate => -a,
            ModOp.Max => Math.Max(a, b),
            ModOp.Min => Math.Min(a, b),
            ModOp.Replace => b,
            ModOp.Zero => 0,
            ModOp.One => 1,
            _ => a
        };
    }
}
