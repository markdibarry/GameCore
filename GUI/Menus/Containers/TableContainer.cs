using System.Linq;
using GameCore.Utility;
using Godot;

namespace GameCore.GUI;

[Tool]
public partial class TableContainer : GridOptionContainer
{
    private bool _columnsDirty;

    protected override void OnChildAdded(Node node)
    {
        base.OnChildAdded(node);
        if (node is not TableRowOption row)
            return;
        row.SortChildren += OnSortChildren;
    }

    protected override void OnChildExiting(Node node)
    {
        base.OnChildExiting(node);
    }

    private void AdjustColumnsWidth()
    {
        TableRowOption? optionItem = (TableRowOption?)OptionItems.FirstOrDefault();
        if (optionItem == null)
            return;
        int columns = optionItem.HBoxContainer.GetChildCount();
        int[] widths = new int[columns];
        TableRowOption[] rows = OptionItems.Cast<TableRowOption>().ToArray();

        foreach (TableRowOption row in rows)
        {
            var children = row.HBoxContainer.GetChildren<Control>().ToArray();
            for (int i = 0; i < children.Length; i++)
            {
                int minWidth = (int)children[i].GetMinimumSize().X;
                if (minWidth > widths[i])
                    widths[i] = minWidth;
            }
        }

        foreach (TableRowOption row in rows)
        {
            var children = row.HBoxContainer.GetChildren<Control>().ToArray();
            for (int i = 0; i < children.Length; i++)
            {
                children[i].Size = children[i].Size with { X = widths[i] };
            }
        }

        _columnsDirty = false;
    }

    private void OnSortChildren()
    {
        if (_columnsDirty)
            return;
        _columnsDirty = true;
        CallDeferred(nameof(AdjustColumnsWidth));
    }
}
