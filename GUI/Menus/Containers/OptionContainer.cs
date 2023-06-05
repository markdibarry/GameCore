using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace GameCore.GUI;

[Tool]
public abstract partial class OptionContainer : MarginContainer
{
    private readonly PackedScene _cursorScene = GD.Load<PackedScene>(HandCursor.GetScenePath());
    public bool AllSelected { get; set; }
    [Export(PropertyHint.Flags)]
    public OptionMode CurrentOptionMode { get; set; } = OptionMode.Single;
    public int PreviousIndex { get; protected set; }
    public int FocusedIndex { get; protected set; }
    public OptionItem? FocusedItem => OptionItems.ElementAtOrDefault(FocusedIndex);
    [Export] public bool MouseEnabled { get; set; } = true;
    public IList<OptionItem> OptionItems { get; } = new List<OptionItem>();
    public Vector2 WindowSize { get; set; }
    public Control? Overlay { get; set; }
    public event Action<OptionContainer, Direction>? FocusOOB;
    public event Action<OptionContainer, OptionItem?>? ItemFocused;
    public event Action<OptionContainer, OptionItem>? ItemHovered;
    public event Action<OptionContainer, OptionItem>? ItemPressed;
    public event Action<OptionContainer>? ContainerRectChanged;

    public override void _Ready()
    {
        SetNodeReferences();
        SubscribeEvents();
    }

    public abstract void AddOption(OptionItem optionItem);
    public abstract void ClearOptionItems();
    public abstract void FocusDirection(Direction direction);
    public abstract void ResetContainerFocus();

    /// <summary>
    /// Focuses the container with the item index specified.<br/>
    /// If only able to select all options, the index for "all" will be selected.
    /// </summary>
    /// <param name="index"></param>
    public virtual void FocusContainer(int index)
    {
        if (CurrentOptionMode.HasFlag(OptionMode.Single))
        {
            FocusItem(index);
        }
        else if (CurrentOptionMode == OptionMode.All)
        {
            SelectAll();
        }
    }

    public void FocusItem(OptionItem item)
    {
        int index = OptionItems.IndexOf(item);
        if (index == -1)
            return;
        FocusItem(index);
    }

    /// <summary>
    /// Focuses the item with the index specified.
    /// <para>If only able to select all options, the index for "all" will be selected.<br/>
    /// Updates the previous index. Removes focus from previous item.<br/>
    /// Updates the scroll position. <br/>
    /// If "all" is to be focused, all selectable items will be flagged as "selected".<br/>
    /// If the previous item was "all", all selectable items have their "selected" flag removed.<br/>
    /// Invokes the "ItemFocused" event.
    /// </para>
    /// </summary>
    /// <param name="index"></param>
    public void FocusItem(int index)
    {
        if (PreviousIndex != FocusedIndex)
            PreviousIndex = FocusedIndex;
        if (OptionItems.Count == 0)
            return;
        if (FocusedItem != null)
            FocusedItem.Focused = false;
        FocusedIndex = GetValidIndex(index);
        if (FocusedItem != null)
            FocusedItem.Focused = true;
        ItemFocused?.Invoke(this, FocusedItem);
    }

    public IEnumerable<OptionItem> GetSelectedItems() => OptionItems.Where(x => x.IsPressed);

    public bool IsItemInView(OptionItem optionItem)
    {
        Vector2 winSize = WindowSize;
        Vector2 conPos = Position;
        Vector2 itemSize = optionItem.Size;
        Vector2 itemPos = optionItem.Position;
        bool isInV = !(itemPos.Y < -conPos.Y || itemPos.Y + itemSize.Y > -conPos.Y + winSize.Y);
        bool isInH = !(itemPos.X < -conPos.X || itemPos.X > -conPos.X + winSize.X);
        return isInV && isInH;
    }

    public void LeaveContainerFocus()
    {
        foreach (OptionItem item in OptionItems)
            SetItemSelection(item, false);
    }

    public void RefocusItem() => FocusItem(FocusedIndex);

    public void ReplaceChildren(IEnumerable<OptionItem> optionItems)
    {
        ClearOptionItems();
        foreach (OptionItem item in optionItems)
            AddOption(item);
    }

    protected int GetValidIndex(int index)
    {
        return Math.Clamp(index, 0, OptionItems.Count - 1);
    }

    protected bool IsValidIndex(int index) => -1 < index && index < OptionItems.Count;

    protected void RaiseFocusOOB(Direction direction) => FocusOOB?.Invoke(this, direction);

    protected void RaiseItemFocused(OptionItem? optionItem) => ItemFocused?.Invoke(this, optionItem);

    protected void RaiseItemHovered(OptionItem optionItem) => ItemHovered?.Invoke(this, optionItem);

    protected void RaiseItemPressed(OptionItem optionItem) => ItemPressed?.Invoke(this, optionItem);

    protected void RaiseContainerRectChanged() => ContainerRectChanged?.Invoke(this);

    protected void SelectAll()
    {
        AllSelected = true;
        foreach (OptionItem item in OptionItems)
            SetItemSelection(item, !item.Disabled);
    }

    protected virtual void SetNodeReferences() { }

    protected virtual void SubscribeEvents() => ItemRectChanged += OnItemRectChanged;

    protected void UnselectAll()
    {
        AllSelected = false;
        foreach (OptionItem item in OptionItems)
            SetItemSelection(item, false);
    }

    private void OnItemRectChanged()
    {
        foreach (OptionItem item in OptionItems)
        {
            if (item.IsPressed && item.SelectionCursor != null)
                item.SelectionCursor.IsHidden = !IsItemInView(item);
        }
        CallDeferred(nameof(RaiseContainerRectChanged));
    }

    private void SetItemSelection(OptionItem item, bool selected)
    {
        item.IsPressed = selected;

        if (!item.IsPressed)
        {
            item.SelectionCursor?.DisableSelectionMode();
            return;
        }

        if (item.SelectionCursor != null)
        {
            item.SelectionCursor.EnableSelectionMode();
            item.SelectionCursor.IsHidden = !IsItemInView(item);
            return;
        }

        if (Overlay == null)
        {
            Overlay = new();
            AddChild(Overlay);
        }

        var cursor = _cursorScene.Instantiate<OptionCursor>();
        cursor.EnableSelectionMode();
        cursor.IsHidden = !IsItemInView(item);
        item.SelectionCursor = cursor;
        Overlay.AddChild(cursor);
        cursor.MoveToTarget(item);
    }
}
