using GameCore.Input;
using Godot;

namespace GameCore.Actors;

public partial class BaseActorBody
{
    private IInputHandler _inputHandlerInternal = null!;
    private Vector2 _move;

    public int BaseSpeed { get; protected set; }
    public Vector2 Direction { get; set; }
    public double Acceleration { get; set; }
    public double Friction { get; set; }
    public double GroundedGravity { get; }
    public bool IsOmnidirectional { get; protected set; }
    public virtual IInputHandler InputHandler => _inputHandlerInternal;

    public bool IsMovingDown() => Velocity.Dot(UpDirection) < 0;

    public void SetInputHandler(IInputHandler inputHandler) => _inputHandlerInternal = inputHandler;
}
