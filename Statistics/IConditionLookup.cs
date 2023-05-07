using System;

namespace GameCore.Statistics;

public interface IConditionLookup
{
    public Type GetConditionType(int conditionType);
}
