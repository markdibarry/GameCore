using GameCore.Actors;

namespace GameCore.ActionEffects;

public abstract class BaseActionEffectRequest
{
    protected BaseActionEffectRequest(int actionType, int value1, int value2)
    {
        ActionType = actionType;
        Value1 = value1;
        Value2 = value2;
    }

    public abstract BaseActorBody? User { get; }
    public abstract BaseActor[] Targets { get; }
    public int ActionType { get; }
    public int Value1 { get; }
    public int Value2 { get; }
}
