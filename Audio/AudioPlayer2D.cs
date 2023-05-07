using Godot;

namespace GameCore.Audio;

public partial class AudioPlayer2D : AudioStreamPlayer2D
{
    public Node2D? SoundSource { get; set; }
    public ulong TimeStamp { get; set; }

    public override void _Ready()
    {
        Finished += OnFinished;
    }

    public override void _Process(double delta)
    {
        if (SoundSource == null)
            return;
        if (IsInstanceValid(SoundSource))
            GlobalPosition = SoundSource.GlobalPosition;
        else
            SoundSource = null;
    }

    public void Reset()
    {
        SoundSource = null;
        TimeStamp = 0;
        Stream = null;
        Stop();
    }

    private void OnFinished() => SoundSource = null;
}
