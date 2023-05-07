namespace GameCore.Actors;

public interface IStateMachine
{
    void ExitState();
    bool TrySwitchTo<T>() where T : IState;
    void Update(double delta);
}
