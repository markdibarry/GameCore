using System;
using GameCore.GUI;

namespace GameCore;

public class GameState
{
    public bool CutsceneActive { get; private set; }
    public bool LoadingActive { get; private set; }
    public bool MenuActive { get; private set; }
    public bool DialogActive { get; private set; }
    public bool GUIActive => MenuActive || DialogActive;
    public event Action<GameState>? GameStateChanged;

    public void Init(GUIController guiController)
    {
        guiController.GUIStatusChanged += OnGUIStatusChanged;
    }

    public void OnGUIStatusChanged(GUIController guiController)
    {
        MenuActive = guiController.MenuActive;
        CutsceneActive = guiController.DialogActive;
        DialogActive = guiController.DialogActive;
        GameStateChanged?.Invoke(this);
    }
}
