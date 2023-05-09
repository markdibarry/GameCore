using Godot;

namespace GameCore.Audio;

public partial class AudioControllerNull : BaseAudioController
{
    public AudioControllerNull() { }
    public override void _Ready() { }
    public override void PlaySoundFX(AudioStream sound) { }
    public override void PlaySoundFX(string soundPath) { }
    public override void PlaySoundFX(Node2D node2D, AudioStream sound) { }
    public override void PlaySoundFX(Node2D node2D, string soundPath) { }
}
