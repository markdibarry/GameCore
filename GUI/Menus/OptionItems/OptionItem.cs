using System;
using Godot;

namespace GameCore.GUI;

public partial class OptionItem : MarginContainer
{
    private bool _disabled;
    private bool _focused;
    private bool _hovered;
    private bool _pressed;
    public OptionCursor? SelectionCursor { get; set; }
    [Export]
    public bool Disabled
    {
        get => _disabled;
        set
        {
            _disabled = value;
            HandleDisplayStateChange();
        }
    }
    [Export]
    public bool Focused
    {
        get => _focused;
        set
        {
            _focused = value;
            HandleDisplayStateChange();
        }
    }
    public bool IsHovered
    {
        get => _hovered;
        set
        {
            _hovered = value;
            HandleDisplayStateChange();
        }
    }
    public object? OptionData { get; set; }
    public bool IsPressed
    {
        get => _pressed;
        set
        {
            _pressed = value;
            HandleDisplayStateChange();
        }
    }
    public bool Toggleable { get; set; }
    public event Action<OptionItem>? MouseEnteredItem;
    public event Action<OptionItem>? MouseExitedItem;
    public event Action<OptionItem>? FocusRequested;
    public event Action<OptionItem>? Pressed;

    public override void _Ready()
    {
        HandleDisplayStateChange();
        if (MouseFilter != MouseFilterEnum.Ignore)
        {
            MouseEntered += OnMouseEntered;
            MouseExited += OnMouseExited;
        }
    }

    public override void _GuiInput(InputEvent inputEvent)
    {
        if (Disabled ||
            inputEvent is not InputEventMouseButton mouseButton ||
            mouseButton.ButtonIndex != MouseButton.Left)
        {
            inputEvent.Dispose();
            return;
        }

        if (Toggleable)
        {
            if (mouseButton.Pressed)
                IsPressed = !IsPressed;
        }
        else
        {
            IsPressed = mouseButton.Pressed;
        }

        inputEvent.Dispose();
        if (!IsPressed)
            return;
        if (!Focused)
            FocusRequested?.Invoke(this);
        Pressed?.Invoke(this);
    }

    protected virtual void HandleDisplayStateChange()
    {
        if (Disabled)
            DisplayDisabled();
        else if (IsPressed)
            DisplayPressed();
        else if (Focused)
            DisplayFocused();
        else if (IsHovered)
            DisplayHovered();
        else
            DisplayNormal();
    }

    protected virtual void DisplayNormal() => Modulate = Colors.DimGrey;

    protected virtual void DisplayFocused() => Modulate = Godot.Colors.White;

    protected virtual void DisplayDisabled() => Modulate = Colors.DisabledGrey;

    protected virtual void DisplayPressed() => DisplayFocused();

    protected virtual void DisplayHovered() => Modulate = Colors.HoverGrey;

    private void OnMouseEntered()
    {
        if (ProcessMode == ProcessModeEnum.Disabled)
            return;
        IsHovered = true;
        MouseEnteredItem?.Invoke(this);
    }

    private void OnMouseExited()
    {
        IsHovered = false;
    }
}
