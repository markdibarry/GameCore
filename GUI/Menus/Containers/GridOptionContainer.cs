using System;
using GameCore.Utility;
using Godot;

namespace GameCore.GUI;

[Tool]
public partial class GridOptionContainer : OptionContainer
{
    private GridContainer _gridContainer = null!;
    private bool _singleRow;

    [ExportGroup("Selecting")]
    [Export]
    public bool FocusWrap { get; set; } = true;
    [ExportGroup("Sizing")]
    [Export(PropertyHint.Range, "1,20")]
    public int Columns
    {
        get => _gridContainer?.Columns ?? 1;
        set
        {
            if (_gridContainer != null)
                _gridContainer.Columns = value;
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
    private bool IsSingleRow => OptionItems.Count <= _gridContainer.Columns;

    public override void _Ready()
    {
        base._Ready();
        foreach (OptionItem item in _gridContainer.GetChildren<OptionItem>())
            OptionItems.Add(item);
    }

    public override void AddOption(OptionItem optionItem)
    {
        _gridContainer.AddChild(optionItem);
    }

    public override void ClearOptionItems()
    {
        OptionItems.Clear();
        _gridContainer.QueueFreeAllChildren();
    }

    public override void FocusDirection(Direction direction)
    {
        if (OptionItems.Count <= 1)
        {
            RaiseFocusOOB(direction);
            return;
        }

        int nextIndex = -1;

        if (!AllSelected)
        {
            nextIndex = direction switch
            {
                Direction.Up => GetNextIndexUp(),
                Direction.Right => GetNextIndexRight(),
                Direction.Down => GetNextIndexDown(),
                Direction.Left => GetNextIndexLeft(),
                _ => -1
            };
        }

        if (nextIndex != -1) // Index found
        {
            FocusItem(nextIndex);
            return;
        }

        if (!FocusWrap)
        {
            RaiseFocusOOB(direction);
            return;
        }

        nextIndex = direction switch // Get OOB index
        {
            Direction.Up => GetBottomEndIndex(),
            Direction.Down => GetTopEndIndex(),
            Direction.Left => GetRightEndIndex(),
            Direction.Right => GetLeftEndIndex(),
            _ => -1
        };

        if (nextIndex == FocusedIndex) // if no change, still raise OOB event
        {
            RaiseFocusOOB(direction);
            return;
        }

        if (nextIndex == -1)
            SelectAll();
        else if (AllSelected)
            UnselectAll();

        FocusItem(nextIndex);
        RaiseFocusOOB(direction);

        int GetNextIndexUp()
        {
            int nextIndex = FocusedIndex - _gridContainer.Columns;
            return IsValidIndex(nextIndex) ? nextIndex : -1;
        }

        int GetNextIndexDown()
        {
            int nextIndex = FocusedIndex + _gridContainer.Columns;
            return IsValidIndex(nextIndex) ? nextIndex : -1;
        }

        int GetNextIndexLeft()
        {
            int nextIndex = FocusedIndex - 1;
            return IsValidIndex(nextIndex) && FocusedIndex % _gridContainer.Columns != 0 ? nextIndex : -1;
        }

        int GetNextIndexRight()
        {
            int nextIndex = FocusedIndex + 1;
            return IsValidIndex(nextIndex) && (FocusedIndex + 1) % _gridContainer.Columns != 0 ? nextIndex : -1;
        }

        int GetTopEndIndex()
        {
            if (CurrentOptionMode.HasFlag(OptionMode.All) && !AllSelected || !CurrentOptionMode.HasFlag(OptionMode.Single))
                return -1;
            return FocusedIndex % _gridContainer.Columns;
        }

        int GetBottomEndIndex()
        {
            if (CurrentOptionMode.HasFlag(OptionMode.All) && !AllSelected || !CurrentOptionMode.HasFlag(OptionMode.Single))
                return -1;
            int firstRowAdjIndex = FocusedIndex % _gridContainer.Columns;
            int lastIndex = OptionItems.Count - 1;
            int lastRowFirstIndex = lastIndex / _gridContainer.Columns * _gridContainer.Columns;
            return Math.Min(lastRowFirstIndex + firstRowAdjIndex, lastIndex);
        }

        int GetLeftEndIndex()
        {
            if (IsSingleRow)
            {
                if (CurrentOptionMode.HasFlag(OptionMode.All) && !AllSelected || !CurrentOptionMode.HasFlag(OptionMode.Single))
                    return -1;
                else
                    return 0;
            }
            return FocusedIndex / _gridContainer.Columns * _gridContainer.Columns;
        }

        int GetRightEndIndex()
        {
            if (IsSingleRow)
            {
                if (CurrentOptionMode.HasFlag(OptionMode.All) && !AllSelected || !CurrentOptionMode.HasFlag(OptionMode.Single))
                    return -1;
                else
                    return OptionItems.Count - 1;
            }
            return (((FocusedIndex / _gridContainer.Columns) + 1) * _gridContainer.Columns) - 1;
        }
    }

    public override void ResetContainerFocus()
    {
        FocusedIndex = 0;
        _gridContainer.Position = Vector2.Zero;
    }

    protected virtual void OnChildAdded(Node node)
    {
        if (node is not OptionItem optionItem)
            return;
        OptionItems.Add(optionItem);

        if (MouseEnabled)
        {
            optionItem.MouseFilter = MouseFilterEnum.Stop;
            optionItem.MouseEnteredItem += OnEnterItemHover;
            optionItem.FocusRequested += OnItemFocusRequested;
            optionItem.Pressed += OnItemPressed;
        }

        UpdateRows();
    }

    protected virtual void OnChildExiting(Node node)
    {
        if (node is not OptionItem optionItem)
            return;
        OptionItems.Remove(optionItem);
        optionItem.SelectionCursor?.QueueFree();
        UpdateRows();
    }

    protected virtual void OnItemFocusRequested(OptionItem optionItem) => FocusItem(optionItem);

    protected virtual void OnItemPressed(OptionItem optionItem) => RaiseItemPressed(optionItem);

    protected override void SetNodeReferences()
    {
        _gridContainer = GetNode<GridContainer>("GridContainer");
    }

    protected override void SubscribeEvents()
    {
        base.SubscribeEvents();
        _gridContainer.ChildEnteredTree += OnChildAdded;
        _gridContainer.ChildExitingTree += OnChildExiting;
    }

    private void OnEnterItemHover(OptionItem optionItem)
    {
        RaiseItemHovered(optionItem);
    }

    private void UpdateRows()
    {
        if (SingleRow)
            Columns = Math.Max(OptionItems.Count, 1);
    }
}
