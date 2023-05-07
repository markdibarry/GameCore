namespace GameCore.Actors;

public abstract class State : IState
{
    public virtual void Enter() { }
    public abstract void Update(double delta);
    public virtual void Exit() { }
    public virtual bool TrySwitch(IStateMachine stateMachine) => false;
}
