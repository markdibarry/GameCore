using GameCore.Actors;
using GameCore.GUI;
using GameCore.Input;
using GameCore.Utility;
using Godot;

namespace GameCore.Events;

public partial class DialogArea : ContextArea
{
    public static string GetScenePath() => GDEx.GetScenePath();
    private static readonly GUIController s_guiController = Locator.Root.GUIController;
    private static readonly GameState s_gameState = Locator.Root.GameState;
    private static readonly IGUIInputHandler s_menuInput = Locator.Root.MenuInput;
    private ColorRect _colorRect = null!;
    private string _dialogPath = string.Empty;

    [Export(PropertyHint.File, "*.json")]
    public string DialogPath
    {
        get => _dialogPath;
        set
        {
            _dialogPath = value;
            if (!FileAccess.FileExists(value))
                IsActive = false;
        }
    }
    [Export]
    public bool Hint
    {
        get => _colorRect?.Visible ?? false;
        set
        {
            if (_colorRect != null)
                _colorRect.Visible = value;
        }
    }

    public override void _Ready()
    {
        _colorRect = GetNode<ColorRect>("ColorRect");
        base._Ready();
    }

    public override void TriggerContext(BaseActorBody actor)
    {
        if (!IsActive
            || s_gameState.GUIActive
            || !s_menuInput.Accept.IsActionJustPressed)
            return;
        _ = s_guiController?.OpenDialogAsync(DialogPath);
    }
}
