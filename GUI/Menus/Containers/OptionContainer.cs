﻿using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace GameCore.GUI;

[Tool]
public abstract partial class OptionContainer : MarginContainer
{
    public const int AllSelectedIndex = -1;
    public bool AllSelected => FocusedIndex == AllSelectedIndex;
    public bool AllOptionEnabled { get; set; }
    public bool SingleOptionsEnabled { get; set; }
    public int PreviousIndex { get; protected set; }
    public int FocusedIndex { get; protected set; }
    public OptionItem? FocusedItem => OptionItems.ElementAtOrDefault(FocusedIndex);
    public IList<OptionItem> OptionItems { get; } = new List<OptionItem>();
    public event Action<OptionContainer>? ContainerUpdated;
    public event Action<OptionContainer, Direction>? FocusOOB;
    public event Action? ItemFocused;
    public event Action<OptionItem>? ItemSelectionChanged;

    /// <summary>
    /// Focuses the container with the item index specified.<br/>
    /// If only able to select all options, the index for "all" will be selected.
    /// </summary>
    /// <param name="index"></param>
    public virtual void FocusContainer(int index)
    {
        if (SingleOptionsEnabled)
        {
            FocusItem(index);
        }
        else if (AllOptionEnabled)
        {
            FocusedIndex = AllSelectedIndex;
            FocusItem(AllSelectedIndex);
        }
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
        if (!SingleOptionsEnabled)
        {
            if (!AllOptionEnabled)
                return;
            index = AllSelectedIndex;
        }
        PreviousIndex = FocusedIndex;
        if (OptionItems.Count == 0)
            return;
        if (FocusedItem != null)
            FocusedItem.Focused = false;
        FocusedIndex = GetValidIndex(index);
        HandleSelectAll();
        if (FocusedItem != null)
            FocusedItem.Focused = true;
        ItemFocused?.Invoke();
    }

    public IEnumerable<OptionItem> GetSelectedItems() => OptionItems.Where(x => x.Selected);

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
        int lowest = AllOptionEnabled ? AllSelectedIndex : 0;
        return Math.Clamp(index, lowest, OptionItems.Count - 1);
    }

    protected bool IsValidIndex(int index) => -1 < index && index < OptionItems.Count;

    private void SetItemSelection(OptionItem item, bool selected)
    {
        item.Selected = selected;
        ItemSelectionChanged?.Invoke(item);
    }

    private void HandleSelectAll()
    {
        if (!AllOptionEnabled)
            return;
        if (AllSelected)
        {
            foreach (OptionItem item in OptionItems)
                SetItemSelection(item, !item.Disabled);
        }
        else if (PreviousIndex == AllSelectedIndex)
        {
            foreach (OptionItem item in OptionItems)
                SetItemSelection(item, false);
        }
    }

    public abstract void AddOption(OptionItem optionItem);
    public abstract void ClearOptionItems();
    public abstract void FocusDirection(Direction direction);
    public abstract void ResetContainerFocus();
    protected void RaiseContainerUpdated() => ContainerUpdated?.Invoke(this);
    protected void RaiseFocusOOB(Direction direction) => FocusOOB?.Invoke(this, direction);
    protected void RaiseItemFocused() => ItemFocused?.Invoke();
}
