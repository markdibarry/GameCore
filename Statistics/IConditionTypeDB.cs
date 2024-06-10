using System;

namespace GameCore.Statistics;

public interface IConditionTypeDB
{
    Type? GetConditionType(string conditionType);
}
