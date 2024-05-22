using Godot;

namespace GameCore.Utility;

public static class Node2DExtensions
{
    public static void FlipScaleX(this Node2D node2D)
    {
        node2D.Scale = new(-node2D.Scale.X, node2D.Scale.Y);
    }

    public static void FlipScaleY(this Node2D node2D)
    {
        node2D.Scale = new(node2D.Scale.X, -node2D.Scale.Y);
    }

    public static Vector2 GetFrameSize(this Sprite2D sprite2D)
    {
        if (sprite2D.Texture == null)
            return default;

        Vector2 textureSize = sprite2D.Texture.GetSize();

        if (textureSize == default)
            return textureSize;

        return new(textureSize.X / sprite2D.Hframes, textureSize.Y / sprite2D.Vframes);
    }
}
