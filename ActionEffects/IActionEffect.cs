using System.Collections.Generic;
using System.Threading.Tasks;
using GameCore.Actors;

namespace GameCore.ActionEffects;

public interface IActionEffect
{
    bool IsActionSequence { get; }
    int TargetType { get; }
    bool CanUse(AActor? user, IList<AActor> targets, int actionType, int value1, int value2);
    Task Use(AActor? user, IList<AActor> targets, int actionType, int value1, int value2);
}
