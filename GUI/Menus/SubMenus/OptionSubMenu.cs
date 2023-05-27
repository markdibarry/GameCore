using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    protected string FocusedSoundPath { get; set; } = Config.GUIFocusSoundPath;

    public sealed override async Task<bool> ResumeSubMenu()
    {
        if (CurrentContainer == null || !await base.ResumeSubMenu())
            return false;
        FocusContainer(CurrentContainer);
        Foreground.Modulate = Godot.Colors.White;
        return true;
    }

    public sealed override bool SuspendSubMenu()
    {
        if (Foreground == null || !base.SuspendSubMenu())
            return false;
        Foreground.Modulate = Godot.Colors.White.Darkened(0.3f);
        return true;
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
        SetContainer(optionContainer);
        optionContainer.FocusContainer(index);
    }

    protected void FocusContainerClosestItem(OptionContainer optionContainer)
    {
        if (CurrentContainer?.FocusedItem == null)
            return;
        int index = optionContainer.OptionItems.GetClosestIndex(CurrentContainer.FocusedItem);
        FocusContainer(optionContainer, index);
    }

    /// <summary>
    /// A callback for when a new container is focused
    /// </summary>
    /// <param name="optionContainer"></param>
    protected virtual void OnContainerFocused(OptionContainer optionContainer) { }

    /// <summary>
    /// A callback for when an item was attempted to be focused out of bounds.
    /// </summary>
    /// <param name="container"></param>
    /// <param name="direction"></param>
    protected virtual void OnOOBFocused(OptionContainer container, Direction direction) { }

    /// <summary>
    /// A callback for when an item was focused.
    /// </summary>
    /// <param name="optionContainer"></param>
    /// <param name="optionItem"></param>
    protected virtual void OnItemFocused(OptionContainer optionContainer, OptionItem? optionItem) { }

    /// <summary>
    /// A callback for when an item was selected.
    /// </summary>
    protected virtual void OnSelectPressed() { }

    protected sealed override void OnPostSetupInternal()
    {
        if (_optionContainers.Count == 0)
            return;
        FocusContainer(_optionContainers.First());
        base.OnPostSetupInternal();
    }

    protected sealed override void SetNodeReferencesInternal()
    {
        base.SetNodeReferencesInternal();
        _cursorsContainer = Foreground.GetNode<Control>("Cursors");
        AddCursor();
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
        optionContainer.ItemFocused += OnItemFocusedInternal;
        optionContainer.ItemSelectionChanged += OnItemSelectionChanged;
        optionContainer.FocusOOB += OnOOBFocused;
    }

    private void MoveCursorToItem(OptionItem optionItem)
    {
        _cursor.MoveToTarget(optionItem);
    }

    private void OnItemFocusedInternal(OptionContainer optionContainer, OptionItem? optionItem)
    {
        _cursor.Visible = optionItem != null;
        if (CurrentContainer != null
            && (optionContainer != CurrentContainer || optionContainer.FocusedIndex != optionContainer.PreviousIndex))
            Audio.PlaySoundFX(FocusedSoundPath);
        if (CurrentContainer != optionContainer)
            SetContainer(optionContainer);
        if (optionItem != null)
            MoveCursorToItem(optionItem);
        OnItemFocused(optionContainer, optionItem);
    }

    private void OnItemSelectionChanged(OptionItem optionItem)
    {
        if (!optionItem.IsPressed)
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

    private void OnSelectPressedInternal()
    {
        //if (CurrentContainer == null || !CurrentContainer.AllSelected &&
        //    (CurrentContainer.FocusedItem == null || CurrentContainer.FocusedItem.Disabled))
        //{
        //    Audio.PlaySoundFX(FocusedSoundPath);
        //    return;
        //}

        //Audio.PlaySoundFX(SelectedSoundPath);
        OnSelectPressed();
    }

    private void SetContainer(OptionContainer optionContainer)
    {
        CurrentContainer?.LeaveContainerFocus();
        CurrentContainer = optionContainer;
        OnContainerFocused(optionContainer);
    }
}
