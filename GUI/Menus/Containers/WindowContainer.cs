using System;
using System.Linq;
using Godot;

namespace GameCore.GUI;

[Tool]
public partial class WindowContainer : MarginContainer
{
    private bool _sizeDirty;
    private MarginContainer _arrows = null!;
    private TextureRect _arrowUp = null!;
    private TextureRect _arrowDown = null!;
    private TextureRect _arrowLeft = null!;
    private TextureRect _arrowRight = null!;
    private Control _scrollBars = null!;
    private HScrollBar _hScrollBar = null!;
    private VScrollBar _vScrollBar = null!;
    public Container ClipContainer { get; set; } = null!;
    public Control? ClipContent { get; set; }
    public Vector2 Padding { get; set; }
    [Export]
    public bool ScrollBarEnabled
    {
        get => _scrollBars.Visible;
        set
        {
            if (_scrollBars != null)
            {
                _scrollBars.Visible = value;
                _arrows.Visible = !value;
            }
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

    public override void _Ready() => Init();

    public override void _Process(double delta)
    {
        if (_sizeDirty)
            HandleSizeDirty();
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
        // Adjust Right
        if (ClipContainer.GlobalPosition.X + ClipContainer.Size.X < control.GlobalPosition.X + control.Size.X)
            position.X = (control.Position.X + control.Size.X - ClipContainer.Size.X) * -1;
        // Adjust Down
        if (ClipContainer.GlobalPosition.Y + ClipContainer.Size.Y < control.GlobalPosition.Y + control.Size.Y)
            position.Y = (control.Position.Y + control.Size.Y - ClipContainer.Size.Y) * -1;
        // Adjust Left
        if (ClipContainer.GlobalPosition.X > control.GlobalPosition.X)
            position.X = -control.Position.X;
        // Adjust Up
        if (ClipContainer.GlobalPosition.Y > control.GlobalPosition.Y)
            position.Y = -control.Position.Y;
        return position;
    }

    private void HandleSizeDirty()
    {
        _sizeDirty = false;
        UpdateArrowVisiblity();
        UpdateScrollBars();
    }

    private void Init()
    {
        SetNodeReferences();
        SubscribeEvents();
        int leftMargin = GetThemeConstant("margin_left");
        int padding = leftMargin * 2;
        Padding = new Vector2(padding, padding);
        OverrideMargin(leftMargin);
    }

    private void OnChildEnteredTree(Node node)
    {
        if (node is OptionContainer optionContainer)
            optionContainer.ItemFocused += OnItemFocused;
        if (node is Control control)
        {
            control.ItemRectChanged += OnContentChanged;
            ClipContent = control;
        }
    }

    private void OnChildExitingTree(Node node)
    {
        if (node is OptionContainer optionContainer)
            optionContainer.ItemFocused -= OnItemFocused;
        ClipContent = null;
        if (ClipContainer.GetChildCount() > 1)
            ClipContent = ClipContainer.GetChildren().First(x => x != node) as Control;
    }

    private void OnContentChanged() => _sizeDirty = true;

    private void OnItemFocused()
    {
        if (ClipContent is not OptionContainer optionContainer)
            return;
        optionContainer.Position = GetScrollPosition(optionContainer.FocusedItem);
    }

    private void OnScrollChanged(double value)
    {
        if (ClipContent == null)
            return;
        float x = -ClipContent.Size.X * (float)(_hScrollBar.Value * 0.01);
        float y = -ClipContent.Size.Y * (float)(_vScrollBar.Value * 0.01);
        ClipContent.Position = new Vector2(x, y);
    }

    private void OnThemeChanged()
    {
        int leftMargin = GetThemeConstant("margin_left");
        Padding = new Vector2(leftMargin * 2, leftMargin * 2);
        OverrideMargin(leftMargin);
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
        _arrows = GetNode<MarginContainer>("Arrows");
        _arrowUp = _arrows.GetNode<TextureRect>("ArrowUp");
        _arrowDown = _arrows.GetNode<TextureRect>("ArrowDown");
        _arrowLeft = _arrows.GetNode<TextureRect>("ArrowLeft");
        _arrowRight = _arrows.GetNode<TextureRect>("ArrowRight");
        _scrollBars = GetNode<Control>("ScrollBars");
        _hScrollBar = _scrollBars.GetNode<HScrollBar>("HScrollBar");
        _vScrollBar = _scrollBars.GetNode<VScrollBar>("VScrollBar");
        ClipContainer = GetNode<Container>("ClipContainer");
        var children = ClipContainer.GetChildren();
        if (children.Count == 1 && children[0] is Control control)
            ClipContent = control;
    }

    private void SubscribeEvents()
    {
        ThemeChanged += OnThemeChanged;
        _hScrollBar.ValueChanged += OnScrollChanged;
        _vScrollBar.ValueChanged += OnScrollChanged;
        ClipContainer.ChildEnteredTree += OnChildEnteredTree;
        ClipContainer.ChildExitingTree += OnChildExitingTree;
        ClipContainer.SortChildren += OnContentChanged;
        if (ClipContent == null)
            return;
        ClipContent.ItemRectChanged += OnContentChanged;
        if (ClipContent is OptionContainer optionContainer)
            optionContainer.ItemFocused += OnItemFocused;
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
