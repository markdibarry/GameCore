using System.Collections.Generic;

namespace GameCore.Actors.Behavior;

public abstract class BaseBehaviorTree
{
    protected BaseBehaviorTree(BaseActorBody actorBody)
    {
        _blackBoard = new();
        _root = SetupTree();
        _root.SetDependencies(actorBody, _blackBoard);
    }

    private readonly BaseBTNode _root;
    private readonly Dictionary<string, object> _blackBoard;

    public void Update(double delta) => _root?.Evaluate(delta);

    public void ClearBlackBoard() => _blackBoard.Clear();

    protected abstract BaseBTNode SetupTree();
}
