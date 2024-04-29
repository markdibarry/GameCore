namespace GameCore.Actors;

public interface IStateController
{
    public bool BaseActionDisabled { get; set; }
    public void Init();
    public void UpdateStates(double delta);
}
