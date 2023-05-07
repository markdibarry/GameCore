using System.Collections.Generic;

namespace GameCore.Actors.Behavior;

public class Selector : ABTNode
{
    public Selector(List<ABTNode> children)
        : base(children)
    { }

    public override NodeState Evaluate(double delta)
    {
        foreach (ABTNode node in Children)
        {
            switch (node.Evaluate(delta))
            {
                case NodeState.Success:
                    return NodeState.Success;
                case NodeState.Running:
                    return NodeState.Running;
            }
        }

        return NodeState.Failure;
    }
}
