using System;
using System.Collections.Generic;
using System.Linq;

namespace GameCore.Actors;

public abstract class StateMachine<TState> : IStateMachine
    where TState : IState
{
    public StateMachine(TState[] states)
    {
        _states = ToStatesDictionary(states);
        FallbackState = states.First();
        State = FallbackState;
    }

    /// <summary>
    /// Cache of the states
    /// </summary>
    /// <returns></returns>
    private readonly Dictionary<Type, TState> _states = new();
    /// <summary>
    /// The current State.
    /// </summary>
    /// <value></value>
    public TState State { get; set; }
    /// <summary>
    /// State to return to as fallback
    /// </summary>
    /// <value></value>
    public TState FallbackState { get; set; }

    /// <summary>
    /// Switches to the FallbackState
    /// </summary>
    public void Reset() => SwitchTo(FallbackState);

    /// <summary>
    /// Exits the State
    /// </summary>
    public void ExitState() => State.Exit();

    /// <summary>
    /// Attempts to switch to a provided state
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public bool TrySwitchTo<T>() where T : IState
    {
        if (!_states.TryGetValue(typeof(T), out TState? state))
            return false;
        SwitchTo(state);
        return true;
    }

    /// <summary>
    /// Updates the State.
    /// </summary>
    public void Update(double delta)
    {
        if (!State.TrySwitch(this))
            State.Update(delta);
    }

    /// <summary>
    /// Switches the current state.
    /// Calls Exit of previous State and enter of new State.
    /// </summary>
    /// <param name="newState"></param>
    private void SwitchTo(TState newState)
    {
        State.Exit();
        State = newState;
        State.Enter();
    }

    /// <summary>
    /// Converts state Array to Dictionary
    /// </summary>
    /// <param name="states"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private static Dictionary<Type, TState> ToStatesDictionary(TState[] states)
    {
        if (states.Length == 0)
            throw new Exception();
        return states.ToDictionary(x => x.GetType(), x => x);
    }
}
