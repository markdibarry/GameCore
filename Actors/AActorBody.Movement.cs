using GameCore.Input;
using GameCore.Utility;
using Godot;

namespace GameCore.Actors;

public partial class AActorBody
{
    private InputHandler _inputHandlerInternal = null!;
    private Vector2 _floatPosition;
    private Vector2 _move;

    public int WalkSpeed { get; protected set; }
    public Vector2 Direction { get; private set; }
    protected double Acceleration { get; set; }
    protected double Friction { get; set; }
    public double GroundedGravity { get; }
    public int MaxSpeed { get; set; }
    public float VelocityX
    {
        get => Velocity.X;
        set => Velocity = new(value, Velocity.Y);
    }
    public float VelocityY
    {
        get => Velocity.Y;
        set => Velocity = new(Velocity.X, value);
    }
    public int IsHalfSpeed { get; set; }
    public bool IsFloater { get; protected set; }
    public int IsRunStuck { get; set; }
    public int RunSpeed => (int)(WalkSpeed * 1.5);
    public virtual InputHandler InputHandler => _inputHandlerInternal;

    public void ChangeDirectionX()
    {
        Direction = Direction.SetX(Direction.X * -1);
        Body.FlipScaleX();
    }

    public bool IsMovingDown() => Velocity.Dot(UpDirection) < 0;

    public void Move()
    {
        _move = IsFloater ? InputHandler.GetLeftAxis() : Direction;
    }

    public void SetInputHandler(InputHandler inputHandler) => _inputHandlerInternal = inputHandler;

    public void UpdateDirection()
    {
        Vector2 velocity = InputHandler.GetLeftAxis().GDExSign();
        if (velocity.X != 0 && velocity.X != Direction.X)
        {
            Direction = Direction.SetX(velocity.X);
            Body.FlipScaleX();
        }
        if (IsFloater && velocity.Y != 0 && velocity.Y != Direction.Y)
            Direction = Direction.SetY(velocity.Y);
    }

    private void HandleMove(double delta)
    {
        int maxSpeed = IsHalfSpeed > 0 ? (int)(MaxSpeed * 0.5) : MaxSpeed;
        Vector2 newVelocity = Velocity;
        if (_move != Vector2.Zero)
        {
            newVelocity.X = VelocityX.MoveToward(_move.X * maxSpeed, Acceleration * delta);
            if (IsFloater)
                newVelocity.Y = VelocityY.MoveToward(_move.Y * maxSpeed, Acceleration * delta);
        }
        else
        {
            newVelocity.X = VelocityX.MoveToward(0, Friction * delta);
            if (IsFloater)
                newVelocity.Y = VelocityY.MoveToward(0, Friction * delta);
        }
        Velocity = newVelocity;
        MoveAndSlide();
    }
}
