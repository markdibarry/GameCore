using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameCore.Input;
using GameCore.Utility;
using Godot;

namespace GameCore.GUI;

[Tool]
public partial class Menu : GUILayer, IMenu
{
    public Stack<SubMenu> SubMenus { get; private set; } = new();
    protected CanvasGroup ContentGroup { get; private set; } = null!;
    protected Control Background { get; private set; } = null!;
    protected Control SubMenuContainer { get; private set; } = null!;
    private SubMenu? CurrentSubMenu => SubMenus.Count > 0 ? SubMenus.Peek() : null;

    public override void _Ready()
    {
        ContentGroup = GetNode<CanvasGroup>("ContentGroup");
        Background = ContentGroup.GetNode<Control>("Content/Background");
        SubMenuContainer = ContentGroup.GetNode<Control>("Content/SubMenus");
        SubMenus = new(SubMenuContainer.GetChildren<SubMenu>());
        if (this.IsToolDebugMode())
            _ = InitAsync(null!);
    }

    public async Task CloseSubMenuAsync(Type? cascadeTo = null, bool preventAnimation = false, object? data = null)
    {
        await CurrentSubMenu!.TransitionCloseAsync(preventAnimation);
        SubMenuContainer.RemoveChild(CurrentSubMenu);
        CurrentSubMenu.QueueFree();
        SubMenus.Pop();
        if (CurrentSubMenu == null)
        {
            await GUIController.CloseLayerAsync(preventAnimation, data);
            return;
        }
        CurrentSubMenu.ResumeSubMenu();
        if (cascadeTo != null && cascadeTo != CurrentSubMenu.GetType())
            await CloseSubMenuAsync(cascadeTo, preventAnimation, data);
        else
            CurrentSubMenu.UpdateData(data);
    }

    public override async Task CloseAsync(bool preventAnimation = false)
    {
        CurrentState = State.Closing;
        if (!preventAnimation)
            await AnimateCloseAsync();
        CurrentState = State.Closed;
    }

    public override void HandleInput(GUIInputHandler menuInput, double delta)
    {
        if (CurrentState != State.Available || CurrentSubMenu?.CurrentState != SubMenu.State.Available)
            return;
        CurrentSubMenu.HandleInput(menuInput, delta);
    }

    public async Task InitAsync(IGUIController guiController, object? data = null)
    {
        GUIController = guiController;
        await CurrentSubMenu!.InitAsync(guiController, this, data);
        CurrentState = State.Available;
    }

    public async Task OpenSubMenuAsync(string path, bool preventAnimation = false, object? data = null)
    {
        await OpenSubMenuAsync(GD.Load<PackedScene>(path), preventAnimation, data);
    }

    public async Task OpenSubMenuAsync(PackedScene packedScene, bool preventAnimation = false, object? data = null)
    {
        SubMenu? subMenu = null;
        try
        {
            CurrentSubMenu?.SuspendSubMenu();
            subMenu = packedScene.Instantiate<SubMenu>();
            SubMenuContainer.AddChild(subMenu);
            SubMenus.Push(subMenu);
            await subMenu.InitAsync(GUIController, this, data);
        }
        catch (Exception ex)
        {
            GD.PrintErr(ex.Message);
            if (subMenu == null)
                return;
            await CloseSubMenuAsync(null, preventAnimation);
        }
    }

    public override void UpdateData(object? data) => CurrentSubMenu!.UpdateData(data);

    public async Task HideInactiveSubMenus(float fade = 0) => await ToggleInactiveVisibility(false, fade);

    public async Task ShowInactiveSubMenus(float fade = 0) => await ToggleInactiveVisibility(true, fade);

    private async Task ToggleInactiveVisibility(bool show, float fade = 0)
    {
        float alpha = show ? 1 : 0;
        if (fade <= 0)
        {
            Background.Modulate = Background.Modulate with { A = alpha };
            foreach (SubMenu subMenu in SubMenus)
            {
                if (subMenu == SubMenus.Peek())
                    continue;
                subMenu.Modulate = subMenu.Modulate with { A = alpha };
            }
            return;
        }

        Tween tween = CreateTween();
        tween.TweenProperty(Background, PropertyName.Modulate.ToString(), Background.Modulate with { A = alpha }, fade);
        foreach (var subMenu in SubMenus)
        {
            if (subMenu == SubMenus.Peek())
                return;
            tween.Parallel().TweenProperty(subMenu, PropertyName.Modulate.ToString(), subMenu.Modulate with { A = alpha }, fade);
        }
        await ToSignal(tween, Tween.SignalName.Finished);
    }
}
