using System;
using System.Threading.Tasks;
using GameCore.Audio;
using GameCore.Input;
using GameCore.Utility;
using Godot;

namespace GameCore.GUI;

[Tool]
public partial class SubMenu : Control
{
    protected static IAudioService Audio { get; } = Locator.Audio;
    public State CurrentState { get; protected set; }
    protected Control Background { get; private set; } = null!;
    protected MarginContainer Foreground { get; private set; } = null!;
    [Export] protected bool PreventCancel { get; set; }
    [Export] protected bool PreventCloseAll { get; set; }
    protected Color TempColor { get; set; }
    protected IGUIController GUIController { get; private set; } = null!;
    protected IMenu Menu { get; private set; } = null!;
    protected string CloseSoundPath { get; set; } = Config.GUICloseSoundPath;
    protected string OpenSubMenuSoundPath { get; set; } = Config.GUIOpenSoundPath;

    public enum State
    {
        Opening,
        Available,
        Suspended,
        Busy,
        Closing,
        Closed,
    }

    public override void _Ready()
    {
        TempColor = Modulate;
        Modulate = Godot.Colors.Transparent;
        if (this.IsToolDebugMode())
            _ = InitAsync(null!, null!);
    }

    /// <summary>
    /// A method for logic pertaining to mocking data for debugging purposes
    /// </summary>
    protected virtual void OnMockPreSetup() { }

    /// <summary>
    /// Receives custom data from previous layer upon opening.
    /// </summary>
    /// <param name="data"></param>
    protected virtual void OnPreSetup(object? data) { }

    /// <summary>
    /// Receives custom data from previous layer upon closing.
    /// </summary>
    /// <param name="data"></param>
    public virtual void UpdateData(object? data) { }

    public virtual void HandleInput(IGUIInputHandler menuInput, double delta)
    {
        if (menuInput.Cancel.IsActionJustPressed && !PreventCancel)
            _ = CloseSubMenuAsync();
        else if (menuInput.Start.IsActionJustPressed && !PreventCloseAll)
            _ = CloseMenuAsync();
    }

    public async Task InitAsync(IGUIController guiController, IMenu menu, object? data = null)
    {
        GUIController = guiController;
        Menu = menu;
        if (this.IsToolDebugMode())
            OnMockPreSetup();
        else
            OnPreSetup(data);
        SetNodeReferencesInternal();
        OnSetupInternal();
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        Modulate = TempColor;
        await AnimateOpenAsync();
        OnPostSetupInternal();
        CurrentState = State.Available;
    }

    public virtual async Task<bool> ResumeSubMenu()
    {
        if (CurrentState != State.Suspended)
            return false;
        ProcessMode = ProcessModeEnum.Inherit;
        CurrentState = State.Available;
        OnSubMenuResumed();
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        return true;
    }

    public virtual bool SuspendSubMenu()
    {
        if (CurrentState != State.Available)
            return false;
        ProcessMode = ProcessModeEnum.Disabled;
        CurrentState = State.Suspended;
        return true;
    }

    public async Task TransitionCloseAsync(bool preventAnimation = false)
    {
        CurrentState = State.Closing;
        OnCloseSubMenu();
        Audio.PlaySoundFX(CloseSoundPath);
        if (!preventAnimation)
            await AnimateCloseAsync();
        CurrentState = State.Closed;
    }

    protected virtual Task AnimateOpenAsync() => Task.CompletedTask;

    protected virtual Task AnimateCloseAsync() => Task.CompletedTask;

    protected virtual async Task CloseMenuAsync(bool preventAnimation = false, object? data = null)
    {
        await GUIController.CloseLayerAsync(preventAnimation, data);
    }

    protected virtual async Task CloseSubMenuAsync(Type? cascadeTo = null, bool preventAnimation = false, object? data = null)
    {
        await Menu.CloseSubMenuAsync(cascadeTo, preventAnimation, data);
    }

    protected virtual void OnCloseSubMenu() { }

    protected virtual void OnSubMenuResumed() { }

    protected virtual async Task OpenSubMenuAsync(string path, bool preventAnimation = false, object? data = null)
    {
        await Menu.OpenSubMenuAsync(path, preventAnimation, data);
    }

    protected virtual async Task OpenSubMenuAsync(PackedScene packedScene, bool preventAnimation = false, object? data = null)
    {
        await Menu.OpenSubMenuAsync(packedScene, preventAnimation, data);
    }

    protected virtual void OnSetup() { }

    protected virtual void OnSetupInternal() => OnSetup();

    /// <summary>
    /// Logic used for setup after the Controls have adjusted.
    /// </summary>
    /// <returns></returns>
    protected virtual void OnPostSetup() { }

    /// <summary>
    /// Logic used for setup after the Controls have adjusted base method.
    /// </summary>
    /// <returns></returns>
    protected virtual void OnPostSetupInternal() => OnPostSetup();

    protected virtual void SetNodeReferencesInternal()
    {
        Foreground = GetNode<MarginContainer>("Foreground");
        Background = GetNode<Control>("Background");
    }
}
