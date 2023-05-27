using System.Threading.Tasks;
using GameCore.Input;
using Godot;

namespace GameCore.GUI;

public abstract partial class GUILayer : Control
{
    public State CurrentState { get; protected set; }
    public string NameId { get; set; } = string.Empty;
    protected IGUIController GUIController { get; set; } = null!;
    /// <summary>
    /// Handles provided input
    /// </summary>
    /// <param name="menuInput"></param>
    /// <param name="delta"></param>
    public abstract void HandleInput(IGUIInputHandler menuInput, double delta);
    /// <summary>
    /// Handles logic needed to gracefully close layer.
    /// </summary>
    /// <param name="preventAnimation"></param>
    /// <returns></returns>
    public virtual Task CloseAsync(bool preventAnimation = false) => Task.CompletedTask;
    /// <summary>
    /// Animation logic for opening the layer.
    /// </summary>
    /// <returns></returns>
    public virtual Task AnimateOpenAsync() => Task.CompletedTask;
    /// <summary>
    /// Animation logic for closing the layer
    /// </summary>
    /// <returns></returns>
    public virtual Task AnimateCloseAsync() => Task.CompletedTask;
    /// <summary>
    /// Passes custom data to next layer on close.
    /// </summary>
    /// <param name="data"></param>
    public abstract void UpdateData(object? data);
    public enum State
    {
        Opening,
        Available,
        Busy,
        Closing,
        Closed
    }
}
