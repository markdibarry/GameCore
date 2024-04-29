namespace GameCore.Actors;

public interface IState
{
    void Enter();
    void Update(double delta);
    void Exit();
}
