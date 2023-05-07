using GameCore.Utility;
using Godot;

namespace GameCore.Statistics;

public partial class AreaBoxContainer : Node2D
{
    public void SetMonitoringDeferred(bool value, bool force = false)
    {
        foreach (AreaBox areaBox in this.GetChildren<AreaBox>())
            areaBox.SetMonitoringDeferred(value, force);
    }

    public void SetMonitorableDeferred(bool value, bool force = false)
    {
        foreach (AreaBox areaBox in this.GetChildren<AreaBox>())
            areaBox.SetMonitorableDeferred(value, force);
    }
}
