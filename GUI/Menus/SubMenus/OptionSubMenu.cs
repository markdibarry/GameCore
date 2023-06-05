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
    /// A callback for when an item was hovered.
    /// </summary>
    /// <param name="optionContainer"></param>
    /// <param name="optionItem"></param>
    protected virtual void OnItemHovered(OptionContainer optionContainer, OptionItem optionItem) { }

    /// <summary>
    /// A callback for when an item was selected.
    /// </summary>
    protected virtual void OnItemPressed(OptionContainer optionContainer, OptionItem optionItem) { }

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
        optionContainer.ItemHovered += OnItemHovered;
        optionContainer.ItemPressed += OnItemPressed;
        optionContainer.FocusOOB += OnOOBFocusedInternal;
        optionContainer.ContainerRectChanged += OnContainerRectChanged;
    }

    private void MoveCursorToItem(OptionItem optionItem)
    {
        _cursor.MoveToTarget(optionItem);
    }

    private void OnContainerRectChanged(OptionContainer optionContainer)
    {
        if (CurrentContainer != optionContainer)
            return;
        if (optionContainer.FocusedItem == null)
            return;

        _cursor.MoveToTarget(optionContainer.FocusedItem);
        _cursor.Visible = optionContainer.IsItemInView(optionContainer.FocusedItem) && !optionContainer.AllSelected;
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

    private void OnItemPressedInternal()
    {
        if (CurrentContainer?.FocusedItem != null)
            OnItemPressed(CurrentContainer, CurrentContainer.FocusedItem);
    }

    private void OnOOBFocusedInternal(OptionContainer container, Direction direction)
    {
        if (container.AllSelected)
            _cursor.Visible = false;
        OnOOBFocused(container, direction);
    }

    private void SetContainer(OptionContainer optionContainer)
    {
        CurrentContainer?.LeaveContainerFocus();
        CurrentContainer = optionContainer;
        OnContainerFocused(optionContainer);
    }
}
