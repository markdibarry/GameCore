using System.Collections.Generic;
using System.Linq;
using Godot;

namespace GameCore.Utility;

public static class NodeExtensions
{
    private static readonly PhysicsPointQueryParameters2D s_params = new()
    {
        CollideWithAreas = true,
    };

    public static T? GetFirstAreaAtGlobalPosition<[MustBeVariant] T>(this Node2D node2d, uint mask)
    {
        var space = node2d.GetWorld2D().DirectSpaceState;
        s_params.Position = node2d.GlobalPosition;
        s_params.CollisionMask = mask;

        Godot.Collections.Array<Godot.Collections.Dictionary> res = space.IntersectPoint(s_params);
        T? result = default;

        foreach (var dict in res)
        {
            if (!dict.TryGetValue("collider", out Variant collider))
                continue;

            result = collider.As<T>();

            if (result != null)
                break;
        }

        return result;
    }

    public static IEnumerable<T> GetChildren<T>(this Node node) where T : Node
    {
        return node.GetChildren().OfType<T>();
    }

    /// <summary>
    /// Returns if the Node is the scene's root.
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public static bool IsSceneRoot(this Node node)
    {
        if (Engine.IsEditorHint())
            return node == node.GetTree().EditedSceneRoot;
        else
            return node == node.GetTree().CurrentScene;
    }

    /// <summary>
    /// Returns if in debug context.
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public static bool IsToolDebugMode(this Node node)
    {
        return Engine.IsEditorHint() || node == node.GetTree().CurrentScene;
    }

    public static void QueueFreeAllChildren(this Node node)
    {
        Godot.Collections.Array<Node> children = node.GetChildren();
        foreach (Node child in children)
        {
            node.RemoveChild(child);
            child.QueueFree();
        }
    }

    public static void QueueFreeAllChildren<T>(this Node node) where T : Node
    {
        IEnumerable<T> children = node.GetChildren<T>();
        foreach (T child in children)
        {
            node.RemoveChild(child);
            child.QueueFree();
        }
    }
}
