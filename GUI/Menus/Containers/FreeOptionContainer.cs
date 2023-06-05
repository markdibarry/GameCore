using GameCore.Utility;
using Godot;

namespace GameCore.GUI;

[Tool]
public partial class FreeOptionContainer : OptionContainer
{
    private Control _optionItemContainer = null!;

    public override void AddOption(OptionItem optionItem)
    {
        OptionItems.Add(optionItem);
        _optionItemContainer.AddChild(optionItem);
    }

    public override void ClearOptionItems()
    {
        OptionItems.Clear();
        _optionItemContainer.QueueFreeAllChildren();
    }

    public override void FocusDirection(Direction direction)
    {
        int nextIndex = FocusedIndex;
        if (direction == Direction.Left || direction == Direction.Up)
            nextIndex--;
        else
            nextIndex++;
        FocusItem(nextIndex);
    }

    public override void ResetContainerFocus() => FocusedIndex = 0;

    protected override void SetNodeReferences()
    {
        _optionItemContainer = GetNode<Control>("OptionItems");
    }
}
