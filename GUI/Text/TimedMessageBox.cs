using GameCore.Utility;
using Godot;

namespace GameCore.GUI;

[Tool]
public partial class TimedMessageBox : MessageBox
{
    public static new string GetScenePath() => GDEx.GetScenePath();
    [Export] public double TimeOut { get; set; } = 2.0;
    private bool _timerFinished;

    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint())
            return;
        if (_timerFinished)
            return;
        if (TimeOut > 0)
        {
            TimeOut -= delta;
        }
        else
        {
            _timerFinished = true;
            TransitionOut();
        }
    }

    public void TransitionOut()
    {
        Tween fadeTween = CreateTween();
        fadeTween.TweenProperty(this, "modulate:a", 0f, 0.1f);
        fadeTween.TweenCallback(Callable.From(QueueFree));
    }
}
