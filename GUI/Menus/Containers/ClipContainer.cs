using System;
using GameCore.Utility;
using Godot;

namespace GameCore.GUI;

[Tool]
public partial class ClipContainer : Container
{
    public ClipContainer()
    {
        ClipContents = true;
        AnchorsPreset = (int)LayoutPreset.FullRect;
        GrowHorizontal = GrowDirection.Both;
        GrowVertical = GrowDirection.Both;
    }

    private bool _clipX;
    private bool _clipY;
    [Export]
    public bool ClipX
    {
        get => _clipX;
        set
        {
            RepositionChildren();
            _clipX = value;
        }
    }
    [Export]
    public bool ClipY
    {
        get => _clipY;
        set
        {
            RepositionChildren();
            _clipY = value;
        }
    }
    [Export] public Vector2 MaxSize { get; set; } = new Vector2(-1, -1);

    public override Vector2 _GetMinimumSize()
    {
        Vector2 max = GetBaseMinimumSize();
        if (!ClipX && MaxSize.X != -1)
            max.X = Math.Min(max.X, MaxSize.X);
        if (!ClipY && MaxSize.Y != -1)
            max.Y = Math.Min(max.Y, MaxSize.Y);
        return max;
    }

    public Vector2 GetBaseMinimumSize()
    {
        Vector2 max = Vector2.Zero;
        foreach (Control control in this.GetChildren<Control>())
        {
            Vector2 size = control.GetCombinedMinimumSize();
            if (!ClipX && size.X > max.X)
                max.X = size.X;
            if (!ClipY && size.Y > max.Y)
                max.Y = size.Y;
        }
        return max;
    }

    public override void _Notification(int what)
    {
        if (what == NotificationSortChildren)
            RepositionChildren();
    }

    public void RepositionChildren()
    {
        UpdateMinimumSize();
        Rect2 rect = new(Vector2.Zero, Size.X, Size.Y);
        foreach (Control child in this.GetChildren<Control>())
            FitChildInRect(child, rect);
    }
}
