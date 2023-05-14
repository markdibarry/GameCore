using System.Collections.Generic;

namespace GameCore.Actors.Behavior;

public class Sequence : BaseBTNode
{
    public Sequence(List<BaseBTNode> children)
        : base(children)
    { }

    public override NodeState Evaluate(double delta)
    {
        foreach (BaseBTNode node in Children)
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
