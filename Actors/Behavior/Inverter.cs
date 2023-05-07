using System.Collections.Generic;

namespace GameCore.Actors.Behavior;

public class Inverter : ABTNode
{
    public Inverter(List<ABTNode> children)
        : base(children)
    { }

    public override NodeState Evaluate(double delta)
    {
        if (Children[0] == null)
            throw new System.ApplicationException("Inverter must have a child node!");

        return Children[0].Evaluate(delta) switch
        {
            NodeState.Failure => NodeState.Success,
            NodeState.Success => NodeState.Failure,
            _ => NodeState.Running,
        };
    }
}
