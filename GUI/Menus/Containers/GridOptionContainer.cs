using System;
using GameCore.Utility;
using Godot;

namespace GameCore.GUI;

[Tool]
public partial class GridOptionContainer : OptionContainer
{
    public GridOptionContainer()
    {
        SingleOptionsEnabled = true;
        FocusWrap = true;
    }

    private bool _singleRow;
    [ExportGroup("Selecting")]
    [Export] public bool DimItems { get; set; }
    [Export] public bool FocusWrap { get; set; }
    [ExportGroup("Sizing")]
    [Export(PropertyHint.Range, "1,20")]
    public int Columns
    {
        get => GridContainer.Columns;
        set
        {
            if (GridContainer != null)
                GridContainer.Columns = value;
        }
    }
    [Export]
    public bool SingleRow
    {
        get => _singleRow;
        set
        {
            _singleRow = value;
            UpdateRows();
        }
    }
    public GridContainer GridContainer { get; set; } = null!;
    private bool IsSingleRow => OptionItems.Count <= GridContainer.Columns;

    public override void _Ready() => Init();

    public override void AddOption(OptionItem optionItem)
    {
        GridContainer.AddChild(optionItem);
    }

    public override void ClearOptionItems()
    {
        OptionItems.Clear();
        GridContainer.QueueFreeAllChildren();
    }

    public override void FocusDirection(Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                FocusUp();
                break;
            case Direction.Right:
                FocusRight();
                break;
            case Direction.Down:
                FocusDown();
                break;
            case Direction.Left:
                FocusLeft();
                break;
        }
    }

    public override void ResetContainerFocus()
    {
        FocusedIndex = 0;
        GridContainer.Position = Vector2.Zero;
    }

    private void FocusUp()
    {
        int currentIndex = FocusedIndex == AllSelectedIndex ? PreviousIndex : FocusedIndex;
        int nextIndex = currentIndex - GridContainer.Columns;
        if (IsValidIndex(nextIndex))
            FocusItem(nextIndex);
        else
            LeaveItemFocus(Direction.Up);
    }

    private void FocusDown()
    {
        int currentIndex = FocusedIndex == AllSelectedIndex ? PreviousIndex : FocusedIndex;
        int nextIndex = currentIndex + GridContainer.Columns;
        if (IsValidIndex(nextIndex))
            FocusItem(nextIndex);
        else
            LeaveItemFocus(Direction.Down);
    }

    private void FocusLeft()
    {
        int currentIndex = FocusedIndex == AllSelectedIndex ? PreviousIndex : FocusedIndex;
        int nextIndex = FocusedIndex - 1;
        if (IsValidIndex(nextIndex) && currentIndex % GridContainer.Columns != 0)
            FocusItem(nextIndex);
        else
            LeaveItemFocus(Direction.Left);
    }

    private void FocusRight()
    {
        int currentIndex = FocusedIndex == AllSelectedIndex ? PreviousIndex : FocusedIndex;
        int nextIndex = FocusedIndex + 1;
        if (IsValidIndex(nextIndex) && (currentIndex + 1) % GridContainer.Columns != 0)
            FocusItem(nextIndex);
        else
            LeaveItemFocus(Direction.Right);
    }

    private void FocusTopEnd()
    {
        if (AllOptionEnabled && !IsSingleRow && FocusedIndex != AllSelectedIndex)
        {
            FocusItem(AllSelectedIndex);
            return;
        }
        int currentIndex = FocusedIndex == AllSelectedIndex ? PreviousIndex : FocusedIndex;
        int nextIndex = currentIndex % GridContainer.Columns;
        if (nextIndex == currentIndex)
            return;
        FocusItem(nextIndex);
    }

    private void FocusBottomEnd()
    {
        if (AllOptionEnabled && !IsSingleRow && FocusedIndex != AllSelectedIndex)
        {
            FocusItem(AllSelectedIndex);
            return;
        }
        int currentIndex = FocusedIndex == AllSelectedIndex ? PreviousIndex : FocusedIndex;
        int firstRowAdjIndex = currentIndex % GridContainer.Columns;
        int lastIndex = OptionItems.Count - 1;
        int lastRowFirstIndex = lastIndex / GridContainer.Columns * GridContainer.Columns;
        int nextIndex = Math.Min(lastRowFirstIndex + firstRowAdjIndex, lastIndex);
        if (nextIndex == currentIndex)
            return;
        FocusItem(nextIndex);
    }

    private void FocusLeftEnd()
    {
        if (IsSingleRow)
        {
            if (AllOptionEnabled && FocusedIndex != AllSelectedIndex)
                FocusItem(AllSelectedIndex);
            else
                FocusItem(0);
            return;
        }
        int currentIndex = FocusedIndex == AllSelectedIndex ? PreviousIndex : FocusedIndex;
        int nextIndex = currentIndex / GridContainer.Columns * GridContainer.Columns;
        if (nextIndex == currentIndex)
            return;
        FocusItem(nextIndex);
    }

    private void FocusRightEnd()
    {
        if (IsSingleRow)
        {
            if (AllOptionEnabled && FocusedIndex != AllSelectedIndex)
                FocusItem(AllSelectedIndex);
            else
                FocusItem(OptionItems.Count - 1);
            return;
        }
        int currentIndex = FocusedIndex == AllSelectedIndex ? PreviousIndex : FocusedIndex;
        int nextIndex = (((currentIndex / GridContainer.Columns) + 1) * GridContainer.Columns) - 1;
        if (nextIndex == currentIndex)
            return;
        FocusItem(nextIndex);
    }

    private void Init()
    {
        SetNodeReferences();
        SubscribeEvents();
        foreach (OptionItem item in GridContainer.GetChildren<OptionItem>())
            OptionItems.Add(item);
    }

    private void LeaveItemFocus(Direction direction)
    {
        if (FocusWrap)
            WrapFocus(direction);
        RaiseFocusOOB(direction);
    }

    private void OnChildAdded(Node node)
    {
        if (node is not OptionItem optionItem)
            return;
        OptionItems.Add(optionItem);
        if (MouseEnabled)
        {
            optionItem.MouseFilter = MouseFilterEnum.Stop;
            optionItem.MouseEnteredItem += OnMouseEnteredItem;
        }

        UpdateRows();
    }

    private void OnChildExiting(Node node)
    {
        if (node is not OptionItem optionItem)
            return;
        OptionItems.Remove(optionItem);
        optionItem.SelectionCursor?.QueueFree();
        UpdateRows();
    }

    private void OnMouseEnteredItem(OptionItem optionItem)
    {
        FocusItem(optionItem);
    }

    private void SetNodeReferences()
    {
        GridContainer = GetNode<GridContainer>("GridContainer");
    }

    private void SubscribeEvents()
    {
        GridContainer.ChildEnteredTree += OnChildAdded;
        GridContainer.ChildExitingTree += OnChildExiting;
    }

    private void UpdateRows()
    {
        if (SingleRow)
            Columns = Math.Max(OptionItems.Count, 1);
    }

    private void WrapFocus(Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                FocusBottomEnd();
                break;
            case Direction.Down:
                FocusTopEnd();
                break;
            case Direction.Left:
                FocusRightEnd();
                break;
            case Direction.Right:
                FocusLeftEnd();
                break;
        }
    }
}
