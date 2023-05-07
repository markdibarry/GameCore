using System.Collections.Generic;

namespace GameCore.Actors.Behavior;

public abstract class ABehaviorTree
{
    protected ABehaviorTree(AActorBody actorBody)
    {
        _blackBoard = new();
        _root = SetupTree();
        _root.SetDependencies(actorBody, _blackBoard);
    }

    private readonly ABTNode _root;
    private readonly Dictionary<string, object> _blackBoard;

    public void Update(double delta) => _root?.Evaluate(delta);

    public void ClearBlackBoard() => _blackBoard.Clear();

    protected abstract ABTNode SetupTree();
}
