using System.Collections.Generic;
using System.Threading.Tasks;
using GameCore.Actors;

namespace GameCore.ActionEffects;

public interface IActionEffect
{
    bool IsActionSequence { get; }
    int TargetType { get; }
    bool CanUse(BaseActor? user, IList<BaseActor> targets, int actionType, int value1, int value2);
    Task Use(BaseActor? user, IList<BaseActor> targets, int actionType, int value1, int value2);
}
