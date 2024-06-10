using System.Collections.Generic;
using GameCore.Utility;

namespace GameCore.Statistics;

public static class ModOp
{
    public const string BaseAdd = "BaseAdd";
    public const string PercentAdd = "PercentAdd";
    public const string Add = "Add";
    public const string PercentMult = "PercentMult";
    public const string Negate = "Negate";
    public const string Min = "Min";
    public const string Max = "Max";
    public const string Replace = "Replace";
    public const string Zero = "Zero";
    public const string One = "One";

    public static readonly Dictionary<string, int> OrderIndexed = new string[]
    {
        BaseAdd,
        PercentAdd,
        Add,
        PercentMult,
        Negate,
        Min,
        Max,
        Replace,
        Zero,
        One
    }.ToIndexedDictionary();
}
