using Godot;

namespace GameCore.GUI;

public partial class OptionItem : MarginContainer
{
    public OptionItem()
    {
        DimWhenUnfocused = true;
    }

    private bool _dimWhenUnfocused;
    private bool _disabled;
    private bool _focused;
    private bool _selected;
    public OptionCursor? SelectionCursor { get; set; }
    [Export]
    public bool DimWhenUnfocused
    {
        get => _dimWhenUnfocused;
        set
        {
            _dimWhenUnfocused = value;
            HandleStateChange();
        }
    }
    [Export]
    public bool Disabled
    {
        get => _disabled;
        set
        {
            _disabled = value;
            HandleStateChange();
        }
    }
    [Export]
    public bool Focused
    {
        get => _focused;
        set
        {
            _focused = value;
            HandleStateChange();
        }
    }

    public object? OptionData { get; set; }
    public bool Selected
    {
        get => _selected;
        set
        {
            _selected = value;
            HandleStateChange();
        }
    }

    public override void _Ready()
    {
        HandleStateChange();
    }

    public void HandleStateChange()
    {
        Color color;
        if (Disabled)
            color = Colors.DisabledGrey;
        else if (Selected || Focused || !DimWhenUnfocused)
            color = Godot.Colors.White;
        else
            color = Colors.DimGrey;
        Modulate = color;
    }
}
