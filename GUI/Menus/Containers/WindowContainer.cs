using System;
using System.Linq;
using Godot;

namespace GameCore.GUI;

[Tool]
public partial class WindowContainer : MarginContainer
{
    public WindowContainer()
    {
        _arrows = new() { MouseFilter = MouseFilterEnum.Ignore };
        Texture2D arrowDownSmall = GD.Load<Texture2D>("res://GameCore/GUI/Cursors/arrowDownSmall.png");
        Texture2D arrowLeftSmall = GD.Load<Texture2D>("res://GameCore/GUI/Cursors/arrowLeftSmall.png");
        _arrowUp = new()
        {
            Visible = false,
            SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
            SizeFlagsVertical = SizeFlags.ShrinkBegin,
            MouseFilter = MouseFilterEnum.Stop,
            Texture = arrowDownSmall,
            StretchMode = TextureRect.StretchModeEnum.Keep,
            FlipV = true
        };
        _arrowDown = new()
        {
            Visible = false,
            SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
            SizeFlagsVertical = SizeFlags.ShrinkEnd,
            MouseFilter = MouseFilterEnum.Stop,
            Texture = arrowDownSmall,
            StretchMode = TextureRect.StretchModeEnum.Keep
        };
        _arrowLeft = new()
        {
            Visible = false,
            SizeFlagsHorizontal = SizeFlags.ShrinkBegin,
            SizeFlagsVertical = SizeFlags.ShrinkCenter,
            MouseFilter = MouseFilterEnum.Stop,
            Texture = arrowLeftSmall,
            StretchMode = TextureRect.StretchModeEnum.Keep
        };
        _arrowRight = new()
        {
            Visible = false,
            SizeFlagsHorizontal = SizeFlags.ShrinkEnd,
            SizeFlagsVertical = SizeFlags.ShrinkCenter,
            MouseFilter = MouseFilterEnum.Stop,
            Texture = arrowLeftSmall,
            StretchMode = TextureRect.StretchModeEnum.Keep,
            FlipH = true
        };

        _arrows.AddChild(_arrowUp);
        _arrows.AddChild(_arrowDown);
        _arrows.AddChild(_arrowLeft);
        _arrows.AddChild(_arrowRight);
        AddChild(_arrows);

        _scrollBars = new()
        {
            Visible = false,
            MouseFilter = MouseFilterEnum.Ignore
        };
        _hScrollBar = new()
        {
            Visible = false,
            Page = 0.65,
            GrowHorizontal = GrowDirection.Both,
            GrowVertical = GrowDirection.Begin,
            SizeFlagsVertical = SizeFlags.ShrinkEnd,
            MouseFilter = MouseFilterEnum.Stop,
            MouseForcePassScrollEvents = false,
        };
        _hScrollBar.SetAnchorsPreset(LayoutPreset.BottomWide);
        _vScrollBar = new()
        {
            Visible = false,
            Page = 0.65,
            GrowHorizontal = GrowDirection.Begin,
            GrowVertical = GrowDirection.Both,
            SizeFlagsVertical = SizeFlags.ShrinkEnd,
            MouseFilter = MouseFilterEnum.Stop,
            MouseForcePassScrollEvents = false
        };

        _vScrollBar.SetAnchorsPreset(LayoutPreset.RightWide);
        _scrollBars.AddChild(_hScrollBar);
        _scrollBars.AddChild(_vScrollBar);
        AddChild(_scrollBars);

        ScrollBarEnabled = _scrollBarEnabled;
    }

    private bool _sizeDirty;
    private bool _scrollBarEnabled;
    private Control? _overlay;
    private MarginContainer _arrows;
    private TextureRect _arrowUp;
    private TextureRect _arrowDown;
    private TextureRect _arrowLeft;
    private TextureRect _arrowRight;
    private Control _scrollBars;
    private HScrollBar _hScrollBar;
    private VScrollBar _vScrollBar;
    public Container ClipContainer { get; set; } = null!;
    public Control? ClipContent { get; set; }
    public Vector2 Padding { get; set; }
    [Export]
    public bool ScrollBarEnabled
    {
        get => _scrollBars.Visible;
        set
        {
            _scrollBars.Visible = value;
            _arrows.Visible = !value;
        }
    }
    [Export]
    public bool FitX
    {
        get => false;
        set
        {
            if (ClipContent == null)
                return;
            Size = Size with { X = GetFittedSize().X };
        }
    }
    [Export]
    public bool FitY
    {
        get => false;
        set
        {
            if (ClipContent == null)
                return;
            Size = Size with { Y = GetFittedSize().Y };
        }
    }

    public override void _Ready()
    {
        SetNodeReferences();
        SubscribeEvents();
        int leftMargin = GetThemeConstant("margin_left");
        int padding = leftMargin * 2;
        Padding = new Vector2(padding, padding);
        OverrideMargin(leftMargin);
    }

    public override void _GuiInput(InputEvent inputEvent)
    {
        if (inputEvent is not InputEventMouseButton mouseEvent || !mouseEvent.Pressed)
        {
            inputEvent.Dispose();
            return;
        }

        switch (mouseEvent.ButtonIndex)
        {
            case MouseButton.WheelDown:
            case MouseButton.WheelUp:
                Scroll(mouseEvent.ButtonIndex);
                break;
        }

        inputEvent.Dispose();
    }

    public override string[] _GetConfigurationWarnings()
    {
        if (ClipContainer.GetChildCount() != 1 || ClipContainer.GetChild(0) is not Control)
            return new string[] { "WindowContainer's ClipContainer child is intended to work with a single child control." };
        return Array.Empty<string>();
    }

    public Vector2 GetFittedSize()
    {
        if (ClipContent == null)
            return Padding;
        Vector2 minSize = ClipContent.GetCombinedMinimumSize();
        return new(minSize.X + Padding.X, minSize.Y + Padding.Y);
    }

    private Vector2 GetScrollPosition(Control? control)
    {
        if (control == null || ClipContent == null)
            return Vector2.Zero;
        Vector2 position = ClipContent.Position;

        // Adjust Vertical
        if (control.Position.Y < -ClipContent.Position.Y)
            position.Y = -control.Position.Y;
        else if (control.Position.Y + control.Size.Y > -ClipContent.Position.Y + ClipContainer.Size.Y)
            position.Y = -(control.Position.Y + control.Size.Y - ClipContainer.Size.Y);

        // Adjust Horizontal
        if (control.Position.X < -ClipContent.Position.X)
            position.X = -control.Position.X;
        else if (control.Position.X + control.Size.X > -ClipContent.Position.X + ClipContainer.Size.X)
            position.X = -(control.Position.X + control.Size.X - ClipContainer.Size.X);

        return position;
    }

    private void HandleSizeDirty()
    {
        _sizeDirty = false;
        UpdateArrowVisiblity();
        UpdateScrollBars();
        if (_overlay != null && ClipContent != null)
            _overlay.Position = ClipContainer.Position + ClipContent.Position;
    }

    private void OnArrowInput(InputEvent inputEvent, Direction direction)
    {
        if (inputEvent is not InputEventMouseButton mouseEvent
            || !mouseEvent.Pressed
            || mouseEvent.ButtonIndex != MouseButton.Left
            || ClipContent is not OptionContainer optionContainer)
        {
            inputEvent.Dispose();
            return;
        }

        ScrollToNextItem(optionContainer, direction);
        inputEvent.Dispose();
    }

    private void OnChildEnteredTree(Node node)
    {
        if (node is not Control control)
            return;
        SetupContent(control);
        ClipContent = control;
    }

    private void OnChildExitingTree(Node node)
    {
        if (node is Control control)
            control.ItemRectChanged -= OnContentChanged;

        if (node is OptionContainer optionContainer)
        {
            optionContainer.ItemFocused -= OnItemFocused;
            _overlay?.QueueFree();
            _overlay = null;
            optionContainer.Overlay = null;
        }

        ClipContent = null;
        if (ClipContainer.GetChildCount() > 1)
            ClipContent = ClipContainer.GetChildren().First(x => x != node) as Control;
    }

    private void OnContentChanged()
    {
        if (_sizeDirty)
            return;
        _sizeDirty = true;
        CallDeferred(nameof(HandleSizeDirty));
    }

    private void OnItemFocused(OptionContainer optionContainer, OptionItem? optionItem)
    {
        optionContainer.Position = GetScrollPosition(optionContainer.FocusedItem);
    }

    private void OnResized()
    {
        if (ClipContent is OptionContainer optionContainer)
            optionContainer.WindowSize = ClipContainer.Size;
    }

    private void OnScrollXChanged(double value)
    {
        if (ClipContent == null)
            return;
        float x = -ClipContent.Size.X * (float)(value * 0.01);
        ClipContent.Position = ClipContent.Position with { X = x };
    }

    private void OnScrollYChanged(double value)
    {
        if (ClipContent == null)
            return;
        float y = -ClipContent.Size.Y * (float)(value * 0.01);
        ClipContent.Position = ClipContent.Position with { Y = y };
    }

    private void OnThemeChanged()
    {
        int leftMargin = GetThemeConstant("margin_left");
        Padding = new Vector2(leftMargin * 2, leftMargin * 2);
        OverrideMargin(leftMargin);
    }

    private void SetupContent(Control control)
    {
        control.ItemRectChanged += OnContentChanged;

        if (control is OptionContainer optionContainer)
        {
            optionContainer.ItemFocused += OnItemFocused;
            _overlay = new();
            AddChild(_overlay);
            optionContainer.Overlay = _overlay;
        }
    }

    private void Scroll(MouseButton mouseButton)
    {
        if (ClipContent is not OptionContainer optionContainer)
            return;
        OptionItem? optionToScroll = null;

        if (optionContainer.Size.Y > ClipContainer.Size.Y)
            optionToScroll = ScrollVertical(optionContainer, mouseButton);
        else if (optionContainer.Size.X > ClipContainer.Size.X)
            optionToScroll = ScrollHorizontal(optionContainer, mouseButton);

        if (optionToScroll == null)
            return;
        optionContainer.Position = GetScrollPosition(optionToScroll);

        OptionItem? ScrollHorizontal(OptionContainer optionContainer, MouseButton mouseButton)
        {
            if (mouseButton == MouseButton.WheelUp)
                return GetNextScrollItemLeft(optionContainer);
            else
                return GetNextScrollItemRight(optionContainer);
        }

        OptionItem? ScrollVertical(OptionContainer optionContainer, MouseButton mouseButton)
        {
            if (mouseButton == MouseButton.WheelUp)
                return GetNextScrollItemAbove(optionContainer);
            else
                return GetNextScrollItemBelow(optionContainer);
        }
    }

    private void ScrollToNextItem(OptionContainer optionContainer, Direction direction)
    {
        OptionItem? optionItem = direction switch
        {
            Direction.Up => GetNextScrollItemAbove(optionContainer),
            Direction.Right => GetNextScrollItemRight(optionContainer),
            Direction.Down => GetNextScrollItemBelow(optionContainer),
            Direction.Left => GetNextScrollItemLeft(optionContainer),
            _ => null
        };
        if (optionItem == null)
            return;
        optionContainer.Position = GetScrollPosition(optionItem);
    }

    private OptionItem? GetNextScrollItemAbove(OptionContainer optionContainer)
    {
        if (optionContainer.Position.Y == 0)
            return null;
        return optionContainer.OptionItems.LastOrDefault(x => x.Position.Y < -optionContainer.Position.Y);
    }

    private OptionItem? GetNextScrollItemBelow(OptionContainer optionContainer)
    {
        float offset = -optionContainer.Position.Y + ClipContainer.Size.Y;
        return optionContainer.OptionItems.FirstOrDefault(x => x.Position.Y + x.Size.Y > offset);
    }

    private OptionItem? GetNextScrollItemLeft(OptionContainer optionContainer)
    {
        if (optionContainer.Position.X == 0)
            return null;
        return optionContainer.OptionItems.LastOrDefault(x => x.Position.X < -optionContainer.Position.X);
    }

    private OptionItem? GetNextScrollItemRight(OptionContainer optionContainer)
    {
        float offset = -optionContainer.Position.X + ClipContainer.Size.X;
        return optionContainer.OptionItems.FirstOrDefault(x => x.Position.X + x.Size.X > offset);
    }

    private void OverrideMargin(int margin)
    {
        _arrows.AddThemeConstantOverride("margin_top", -margin);
        _arrows.AddThemeConstantOverride("margin_right", -margin);
        _arrows.AddThemeConstantOverride("margin_bottom", -margin);
        _arrows.AddThemeConstantOverride("margin_left", -margin);

        _hScrollBar.OffsetLeft = -margin;
        _hScrollBar.OffsetTop = margin;
        _hScrollBar.OffsetBottom = margin;
        _vScrollBar.OffsetTop = -margin;
        _vScrollBar.OffsetLeft = margin;
        _vScrollBar.OffsetRight = margin;
    }

    private void SetNodeReferences()
    {
        ClipContainer = GetNode<Container>("ClipContainer");
        Godot.Collections.Array<Node> children = ClipContainer.GetChildren();
        if (children.Count == 1 && children[0] is Control control)
            ClipContent = control;
    }

    private void SubscribeEvents()
    {
        ThemeChanged += OnThemeChanged;
        _hScrollBar.ValueChanged += OnScrollXChanged;
        _vScrollBar.ValueChanged += OnScrollYChanged;
        _arrowUp.GuiInput += (InputEvent inputEvent) => OnArrowInput(inputEvent, Direction.Up);
        _arrowRight.GuiInput += (InputEvent inputEvent) => OnArrowInput(inputEvent, Direction.Right);
        _arrowDown.GuiInput += (InputEvent inputEvent) => OnArrowInput(inputEvent, Direction.Down);
        _arrowLeft.GuiInput += (InputEvent inputEvent) => OnArrowInput(inputEvent, Direction.Left);
        ClipContainer.ChildEnteredTree += OnChildEnteredTree;
        ClipContainer.ChildExitingTree += OnChildExitingTree;
        ClipContainer.SortChildren += OnContentChanged;
        ClipContainer.Resized += OnResized;

        if (ClipContent != null)
            SetupContent(ClipContent);
    }

    private void UpdateArrowVisiblity()
    {
        if (ScrollBarEnabled)
            return;

        _arrowLeft.Visible = false;
        _arrowRight.Visible = false;
        _arrowUp.Visible = false;
        _arrowDown.Visible = false;

        if (ClipContent == null)
            return;

        // H arrows
        if (ClipContent.Size.X > ClipContainer.Size.X)
        {
            _arrowLeft.Visible = ClipContent.Position.X < 0;
            _arrowRight.Visible = ClipContent.Size.X + ClipContent.Position.X > ClipContainer.Size.X;
        }

        // V arrows
        if (ClipContent.Size.Y > ClipContainer.Size.Y)
        {
            _arrowUp.Visible = ClipContent.Position.Y < 0;
            _arrowDown.Visible = ClipContent.Size.Y + ClipContent.Position.Y > ClipContainer.Size.Y;
        }
    }

    private void UpdateScrollBars()
    {
        if (!ScrollBarEnabled)
            return;

        _hScrollBar.Visible = false;
        _vScrollBar.Visible = false;

        if (ClipContent == null)
            return;

        // HScrollBar
        if (ClipContent.Size.X > ClipContainer.Size.X)
        {
            _hScrollBar.Visible = true;
            _hScrollBar.Page = (ClipContainer.Size.X / ClipContent.Size.X) * 100;
            _hScrollBar.Value = (-ClipContent.Position.X / ClipContent.Size.X) * 100;
        }

        // VScrollBar
        if (ClipContent.Size.Y > ClipContainer.Size.Y)
        {
            _vScrollBar.Visible = true;
            _vScrollBar.Page = (ClipContainer.Size.Y / ClipContent.Size.Y) * 100;
            _vScrollBar.Value = (-ClipContent.Position.Y / ClipContent.Size.Y) * 100;
        }
    }
}
