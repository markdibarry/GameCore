using GameCore.Actors;

namespace GameCore.Events;

public interface IContextArea
{
    void TriggerContext(AActorBody actor);
}
