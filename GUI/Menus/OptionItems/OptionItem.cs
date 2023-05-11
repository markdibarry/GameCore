using System;
using Godot;

namespace GameCore.GUI;

public partial class OptionItem : MarginContainer
{
    private bool _dimWhenUnfocused = true;
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
    public event Action<OptionItem>? MouseEnteredItem;

    public override void _Ready()
    {
        HandleStateChange();
        if (MouseFilter != MouseFilterEnum.Ignore)
            MouseEntered += OnMouseEntered;
    }

    protected virtual void HandleStateChange()
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

    private void OnMouseEntered()
    {
        if (Disabled || ProcessMode == ProcessModeEnum.Disabled)
            return;
        MouseEnteredItem?.Invoke(this);
    }
}
