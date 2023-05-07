using System;
using Godot;

namespace GameCore.Audio;

public abstract partial class AAudioController : Node2D
{
    public AAudioController()
    {
        _sceneFXPlayers = new AudioPlayer2D[MaxFXStreams];
        _menuFXPlayers = new AudioStreamPlayer[MaxFXStreams];
    }

    private const int MaxFXStreams = 8;
    private readonly AudioStreamPlayer[] _menuFXPlayers;
    private readonly AudioPlayer2D[] _sceneFXPlayers;
    private Node2D _sceneFX = null!;
    private Node2D _menuFX = null!;

    public override void _Ready()
    {
        _sceneFX = GetNodeOrNull<Node2D>("SceneFX");
        _menuFX = GetNodeOrNull<Node2D>("MenuFX");
        AddSceneAudioPlayers();
        AddMenuAudioPlayers();
    }

    public void ClearFX()
    {
        foreach (var player in _sceneFXPlayers)
            player.Reset();
    }

    public virtual void PlaySoundFX(string soundName)
    {
        if (string.IsNullOrEmpty(soundName))
            return;
        AudioStream stream = GD.Load<AudioStream>($"{Config.AudioFullPath}{soundName}");
        PlaySoundFX(stream);
    }

    public virtual void PlaySoundFX(AudioStream sound)
    {
        AudioStreamPlayer? audioPlayer = Array.Find(_menuFXPlayers, x => x.Stream == sound);
        if (audioPlayer != null)
        {
            audioPlayer.Play();
            return;
        }
        audioPlayer = GetNextMenuPlayer();
        if (audioPlayer == null)
            return;
        audioPlayer.Stream = sound;
        audioPlayer.Play();
    }

    public virtual void PlaySoundFX(Node2D node2D, string soundName)
    {
        if (string.IsNullOrEmpty(soundName))
            return;
        AudioStream stream = GD.Load<AudioStream>($"{Config.AudioFullPath}{soundName}");
        PlaySoundFX(node2D, stream);
    }

    public virtual void PlaySoundFX(Node2D node2D, AudioStream sound)
    {
        AudioPlayer2D? audioPlayer = Array.Find(_sceneFXPlayers, x => x.Stream == sound);
        if (audioPlayer != null)
        {
            audioPlayer.SoundSource = node2D;
            audioPlayer.Play();
            return;
        }
        audioPlayer = GetNextScenePlayer();
        if (audioPlayer == null)
            return;
        audioPlayer.TimeStamp = Godot.Time.GetTicksMsec();
        audioPlayer.SoundSource = node2D;
        audioPlayer.Stream = sound;
        audioPlayer.Play();
    }

    public void OnGameStateChanged(GameState gameState)
    {
        _sceneFX.ProcessMode = gameState.MenuActive ? ProcessModeEnum.Disabled : ProcessModeEnum.Inherit;
    }

    public void Reset()
    {
        ClearFX();
        OnGameStateChanged(Locator.Root.GameState);
    }

    private void AddSceneAudioPlayers()
    {
        for (int i = 0; i < MaxFXStreams; i++)
        {
            _sceneFXPlayers[i] = new AudioPlayer2D()
            {
                MaxPolyphony = 3,
                Bus = "SceneFX"
            };
            _sceneFX.AddChild(_sceneFXPlayers[i]);
        }
    }

    private void AddMenuAudioPlayers()
    {
        for (int i = 0; i < MaxFXStreams; i++)
        {
            _menuFXPlayers[i] = new AudioStreamPlayer()
            {
                MaxPolyphony = 3,
                Bus = "MenuFX"
            };
            _menuFX.AddChild(_menuFXPlayers[i]);
        }
    }

    private AudioStreamPlayer GetNextMenuPlayer()
    {
        AudioStreamPlayer? audioPlayer = Array.Find(_menuFXPlayers, x => !x.Playing);
        return audioPlayer ?? _menuFXPlayers[0];
    }

    private AudioPlayer2D? GetNextScenePlayer()
    {
        AudioPlayer2D? audioPlayer = Array.Find(_sceneFXPlayers, x => !x.Playing);
        if (audioPlayer != null)
            return audioPlayer;
        audioPlayer = _sceneFXPlayers[0];
        if (audioPlayer == null)
            return null;
        for (int i = 1; i < MaxFXStreams; i++)
        {
            if (_sceneFXPlayers[i].TimeStamp < audioPlayer.TimeStamp)
                audioPlayer = _sceneFXPlayers[i];
        }
        return audioPlayer;
    }
}
