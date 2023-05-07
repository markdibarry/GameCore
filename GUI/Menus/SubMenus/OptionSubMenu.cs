using System;
using System.Collections.Generic;
using System.Linq;
using GameCore.Utility;
using Godot;

namespace GameCore.GUI;

[Tool]
public partial class OptionSubMenu : SubMenu
{
    private readonly PackedScene _cursorScene = GD.Load<PackedScene>(HandCursor.GetScenePath());
    private readonly List<OptionContainer> _optionContainers = new();
    private OptionCursor _cursor = null!;
    private Control _cursorsContainer = null!;

    public OptionContainer? CurrentContainer { get; private set; }
    public IReadOnlyCollection<OptionContainer> OptionContainers => _optionContainers;
    protected string SelectedSoundPath { get; set; } = "menu_select1.wav";
    protected string FocusedSoundPath { get; set; } = "menu_bip1.wav";

    public override async void ResumeSubMenu()
    {
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        if (CurrentContainer == null)
            throw new Exception("Current container is null!");
        FocusContainer(CurrentContainer);
        base.ResumeSubMenu();
    }

    protected void AddContainer(OptionContainer container)
    {
        _optionContainers.Add(container);
        SubscribeToEvents(container);
    }

    protected void FocusContainer(OptionContainer optionContainer)
    {
        FocusContainer(optionContainer, optionContainer.FocusedIndex);
    }

    protected void FocusContainer(OptionContainer optionContainer, int index)
    {
        if (optionContainer.OptionItems.Count == 0)
            return;
        CurrentContainer?.LeaveContainerFocus();
        OnFocusContainer(optionContainer);
        CurrentContainer = optionContainer;
        optionContainer.FocusContainer(index);
    }

    protected void FocusContainerClosestItem(OptionContainer optionContainer)
    {
        if (CurrentContainer?.FocusedItem == null)
            return;
        int index = optionContainer.OptionItems.GetClosestIndex(CurrentContainer.FocusedItem);
        FocusContainer(optionContainer, index);
    }

    protected virtual void OnFocusContainer(OptionContainer optionContainer) { }

    protected virtual void OnFocusOOB(OptionContainer container, Direction direction) { }

    protected virtual void OnItemFocused() { }

    protected virtual void OnSelectPressed() { }

    protected sealed override void PostWaitFrameSetupBase()
    {
        if (_optionContainers.Count == 0)
            return;
        FocusContainer(_optionContainers.First());
        base.PostWaitFrameSetupBase();
    }

    protected sealed override void SetNodeReferencesBase()
    {
        base.SetNodeReferencesBase();
        _cursorsContainer = Foreground.GetNode<Control>("Cursors");
        AddCursor();
        SetNodeReferences();
    }

    protected void AddCursor()
    {
        OptionCursor? cursor = _cursorsContainer.GetChildren<OptionCursor>().FirstOrDefault();
        if (cursor == null)
        {
            cursor = _cursorScene.Instantiate<OptionCursor>();
            _cursorsContainer.AddChild(cursor);
        }
        _cursor = cursor;
        _cursor.Visible = false;
    }

    protected void SubscribeToEvents(OptionContainer optionContainer)
    {
        optionContainer.ItemFocused += OnItemFocusedBase;
        optionContainer.ItemSelectionChanged += OnItemSelectionChanged;
        optionContainer.FocusOOB += OnFocusOOB;
        optionContainer.ContainerUpdated += OnContainerChanged;
    }

    private void MoveCursorToItem(OptionItem? optionItem)
    {
        if (optionItem == null)
            return;
        _cursor.MoveToTarget(optionItem);
    }

    private void OnContainerChanged(OptionContainer optionContainer)
    {
        if (optionContainer == CurrentContainer)
            MoveCursorToItem(optionContainer.FocusedItem);
    }

    private void OnItemFocusedBase()
    {
        _cursor.Visible = CurrentContainer?.FocusedItem != null;
        if (CurrentContainer != null)
        {
            if (CurrentContainer.FocusedIndex != CurrentContainer.PreviousIndex)
                Audio.PlaySoundFX(FocusedSoundPath);
            MoveCursorToItem(CurrentContainer.FocusedItem);
        }
        OnItemFocused();
    }

    private void OnItemSelectionChanged(OptionItem optionItem)
    {
        if (!optionItem.Selected)
        {
            optionItem.SelectionCursor?.DisableSelectionMode();
            return;
        }

        if (optionItem.SelectionCursor != null)
            return;

        var cursor = _cursorScene.Instantiate<OptionCursor>();
        cursor.EnableSelectionMode();
        optionItem.SelectionCursor = cursor;
        _cursorsContainer.AddChild(cursor);
        cursor.MoveToTarget(optionItem);
    }

    private void OnSelectPressedBase()
    {
        if (CurrentContainer == null || !CurrentContainer.AllSelected &&
            (CurrentContainer.FocusedItem == null || CurrentContainer.FocusedItem.Disabled))
        {
            Audio.PlaySoundFX(FocusedSoundPath);
            return;
        }

        Audio.PlaySoundFX(SelectedSoundPath);
        OnSelectPressed();
    }
}
