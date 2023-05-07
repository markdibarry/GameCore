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

    public static int Compute(this ModOp op, int a, int b)
    {
        return op switch
        {
            ModOp.Add => a + b,
            ModOp.Subtract => a - b,
            ModOp.Multiply => a * b,
            ModOp.Divide => b == 0 ? 0 : a / b,
            ModOp.Percent => (int)(a * (b * 0.01)),
            ModOp.Negate => -a,
            ModOp.Max => System.Math.Max(a, b),
            ModOp.Min => System.Math.Min(a, b),
            ModOp.Replace => b,
            ModOp.Zero => 0,
            ModOp.One => 1,
            ModOp.None or
            _ => a
        };
    }
}
