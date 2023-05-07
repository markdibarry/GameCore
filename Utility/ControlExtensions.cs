using Godot;

namespace GameCore.Utility;

public static class ControlExtensions
{
    public static int GetClosestIndex<T>(this ICollection<T> controls, Control control) where T : Control
    {
        int controlCount = controls.Count;
        if (controlCount == 0)
            return -1;
        if (controlCount == 1)
            return 0;
        int nearestIndex = 0;
        float nearestDistance = control.GlobalPosition.DistanceTo(controls.ElementAt(0).GlobalPosition);
        for (int i = 1; i < controlCount; i++)
        {
            float newDistance = control.GlobalPosition.DistanceTo(controls.ElementAt(i).GlobalPosition);
            if (newDistance < nearestDistance)
            {
                nearestIndex = i;
                nearestDistance = newDistance;
            }
        }
        return nearestIndex;
    }
}
