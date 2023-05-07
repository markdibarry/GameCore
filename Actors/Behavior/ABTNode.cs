﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace GameCore.Actors.Behavior;

public abstract class ABTNode
{
    public enum NodeState { Running, Failure, Success };

    protected ABTNode()
    {
        Parent = null;
        Children = new List<ABTNode>();
    }

    protected ABTNode(List<ABTNode> children)
        : this()
    {
        foreach (ABTNode child in children)
        {
            child.Parent = this;
            Children.Add(child);
        }
    }

    public void SetDependencies(AActorBody actorBody, Dictionary<string, object> blackBoard)
    {
        ActorBodyInternal = actorBody;
        _blackBoard = blackBoard;
        Init();
        foreach (var child in Children)
            child.SetDependencies(actorBody, blackBoard);
    }

    private Dictionary<string, object> _blackBoard = null!;
    protected ABTNode? Parent { get; set; }
    protected virtual AActorBody ActorBody => ActorBodyInternal;
    protected AActorBody ActorBodyInternal { get; private set; } = null!;
    protected List<ABTNode> Children { get; }

    public virtual void Init() { }

    public virtual NodeState Evaluate(double delta) => NodeState.Failure;

    protected void SetData(string key, object value)
    {
        _blackBoard[key] = value;
    }

    protected object? GetData(string key)
    {
        if (_blackBoard.TryGetValue(key, out object? value))
            return value;
        return null;
    }

    protected T? GetData<T>(string key)
    {
        if (_blackBoard.TryGetValue(key, out object? value) && value is T t)
            return t;
        return default;
    }

    protected bool TryGetData<T>(string key, [NotNullWhen(returnValue: true)] out T? value)
    {
        if (_blackBoard.TryGetValue(key, out object? result) && result is T t)
        {
            value = t;
            return true;
        }
        value = default;
        return false;
    }

    protected bool RemoveData(string key) => _blackBoard.Remove(key);
}
