﻿using System.Collections.Generic;
using GameCore.Actors;
using GameCore.AreaScenes;
using GameCore.GUI;
using GameCore.Input;
using Godot;

namespace GameCore;

public abstract partial class AGameSession : Node2D
{
    protected Node2D AreaSceneContainer { get; set; } = null!;
    protected AHUD HUD { get; set; } = null!;
    protected GUIController GUIController { get; set; } = null!;
    public AAreaScene? CurrentAreaScene { get; private set; }
    public TransitionLayer Transition { get; private set; } = null!;
    public bool Paused { get; private set; }

    public override void _Ready()
    {
        SetNodeReferences();
    }

    public abstract void HandleInput(GUIInputHandler menuInput, double delta);

    public virtual void AddAreaScene(AAreaScene areaScene)
    {
        if (IsInstanceValid(CurrentAreaScene))
        {
            GD.PrintErr("AreaScene already active. Cannot add new AreaScene.");
            return;
        }

        CurrentAreaScene = areaScene;
        AreaSceneContainer.AddChild(areaScene);
        areaScene.Init(HUD);
    }

    /// <summary>
    /// Initializer for Session. Must cast game save for specific type.
    /// </summary>
    /// <param name="guiController"></param>
    /// <param name="gameSave"></param>
    public abstract void Init(GUIController guiController, IGameSave gameSave);

    public void OnGameStateChanged(GameState gameState)
    {
        if (gameState.MenuActive)
            Pause();
        else
            Resume();
        CurrentAreaScene?.OnGameStateChanged(gameState);
    }

    public void Pause()
    {
        CurrentAreaScene?.Pause();
        HUD.Pause();
        Paused = true;
    }

    public void Resume()
    {
        CurrentAreaScene?.Resume();
        HUD.Resume();
        Paused = false;
    }

    public void RemoveAreaScene()
    {
        if (CurrentAreaScene == null)
            return;

        AreaSceneContainer.RemoveChild(CurrentAreaScene);
        CurrentAreaScene.QueueFree();
        CurrentAreaScene = null;
    }

    public void StartActionSequence(IEnumerable<AActor> actors)
    {
        CurrentAreaScene?.StartActionSequence(actors);
    }

    public void StopActionSequence()
    {
        CurrentAreaScene?.StopActionSequence();
    }

    protected virtual void SetNodeReferences()
    {
        HUD = GetNode<AHUD>("HUD");
        AreaSceneContainer = GetNode<Node2D>("AreaSceneContainer");
        Transition = GetNode<TransitionLayer>("Transition");
    }
}
