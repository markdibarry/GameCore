using Godot;

namespace GameCore.Audio;

public interface IAudioService
{
    void ClearFX();
    void PlaySoundFX(string soundName);
    void PlaySoundFX(AudioStream sound);
    void PlaySoundFX(Node2D node2D, string soundName);
    void PlaySoundFX(Node2D node2D, AudioStream sound);
    void OnGameStateChanged(GameState gameState);
    void Reset();
}
