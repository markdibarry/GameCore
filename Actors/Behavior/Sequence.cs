using System.Collections.Generic;

namespace GameCore.Actors.Behavior;

public class Sequence : ABTNode
{
    public Sequence(List<ABTNode> children)
        : base(children)
    { }

    public override NodeState Evaluate(double delta)
    {
        foreach (ABTNode node in Children)
        {
            switch (node.Evaluate(delta))
            {
                case NodeState.Failure:
                    return NodeState.Failure;
                case NodeState.Running:
                    return NodeState.Running;
            }
        }

        return NodeState.Success;
    }
}
