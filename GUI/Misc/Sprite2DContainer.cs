using System;
using Godot;

namespace GameCore.GUI;

[Tool]
public partial class Sprite2DContainer : Container
{
    public Sprite2D? Sprite2D { get; set; }

    public override void _Ready()
    {
        if (GetChildCount() != 1)
            return;
        Sprite2D = GetChild(0) as Sprite2D;
        if (Sprite2D != null)
            Sprite2D.ItemRectChanged += UpdateSize;
        ChildEnteredTree += OnChildEntered;
        ChildExitingTree += OnChildExiting;
        UpdateSize();
    }

    public override string[] _GetConfigurationWarnings()
    {
        if (GetChildCount() != 1 || GetChild(0) is not Godot.Sprite2D)
            return new string[] { "Sprite2DContainer is intended to work with a single Sprite2D child." };
        return Array.Empty<string>();
    }

    private Vector2I GetNewSize()
    {
        if (Sprite2D == null || !Sprite2D.Visible)
            return Vector2I.Zero;
        float v = 0;
        float h = 0;
        Rect2 rect = Sprite2D.GetRect();
        Vector2 spritePos = Sprite2D.Position + rect.Size;
        if (spritePos.X > h)
            h = spritePos.X;
        if (spritePos.Y > v)
            v = spritePos.Y;
        return new((int)h, (int)v);
    }

    private void OnChildEntered(Node node)
    {
        if (GetChildCount() != 1)
            return;
        Sprite2D = GetChild(0) as Sprite2D;
        if (Sprite2D != null)
            Sprite2D.ItemRectChanged += UpdateSize;
        UpdateSize();
    }

    private void OnChildExiting(Node node)
    {
        if (node == Sprite2D)
            Sprite2D = null;
        if (GetChildCount() <= 1)
            return;
        Sprite2D = GetChild(0) as Sprite2D;
        if (Sprite2D != null)
            Sprite2D.ItemRectChanged += UpdateSize;
        UpdateSize();
    }

    private void UpdateSize() => CustomMinimumSize = GetNewSize();
}
