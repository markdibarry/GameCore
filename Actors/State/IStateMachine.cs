namespace GameCore.Actors;

public interface IStateMachine
{
    void ExitState();
    bool Update(double delta);
}
