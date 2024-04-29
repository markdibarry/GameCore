using System;
using System.Collections.Generic;

namespace GameCore.Actors;

public abstract class StateMachine<TState> : IStateMachine
    where TState : IState
{
    /// <summary>
    /// Cache of the states
    /// </summary>
    /// <returns></returns>
    private readonly Dictionary<Type, TState> _states = [];
    /// <summary>
    /// Cache of the state transitions
    /// </summary>
    private readonly Dictionary<TState, Func<Type?>> _transitions = [];
    /// <summary>
    /// The current State
    /// </summary>
    /// <value></value>
    public TState State { get; private set; } = default!;
    /// <summary>
    /// State to return to as fallback
    /// </summary>
    /// <value></value>
    public TState InitialState { get; private set; } = default!;

    /// <summary>
    /// Switches to the FallbackState
    /// </summary>
    public void Reset() => SwitchTo(InitialState);

    /// <summary>
    /// Exits the State
    /// </summary>
    public void ExitState() => State.Exit();

    /// <summary>
    /// Updates the State
    /// </summary>
    public bool Update(double delta)
    {
        // Get transition for state
        if (_transitions.TryGetValue(State, out Func<Type?>? transition))
        {
            Type? type = transition();

            if (type != null && _states.TryGetValue(type, out TState? state))
            {
                SwitchTo(state);
                return true;
            }
        }

        State.Update(delta);

        return false;
    }

    protected void AddState(TState state, Func<Type?> transition)
    {
        if (_states.Count == 0)
        {
            InitialState = state;
            State = state;
        }

        _transitions.Add(state, transition);
        _states.Add(state.GetType(), state);
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
}
