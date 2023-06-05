using Godot;

namespace GameCore;

public static class Colors
{
    public static readonly Color TextRed = new(1f, 0.7f, 0.7f, 1f);
    public static readonly Color TextGreen = new(0.7f, 1f, 0.7f, 1f);
    public static readonly Color HoverGrey = Godot.Colors.White.Darkened(0.15f);
    public static readonly Color DimGrey = Godot.Colors.White.Darkened(0.3f);
    public static readonly Color DisabledGrey = Godot.Colors.White.Darkened(0.5f);
}

public static class WeaponTypes
{
    public const string LongStick = "LongStick";
    public const string Sword = "Sword";
    public const string Wand = "Wand";
}
