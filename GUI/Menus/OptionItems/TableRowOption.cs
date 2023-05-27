using Godot;

namespace GameCore.GUI;

[Tool]
public partial class TableRowOption : OptionItem
{
    public HBoxContainer HBoxContainer { get; set; } = null!;

    public override void _Ready()
    {
        base._Ready();
        HBoxContainer = GetNode<HBoxContainer>("HBoxContainer");
    }
}
