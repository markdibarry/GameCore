using Godot;

namespace GameCore.Statistics;

public partial class AreaBox : Area2D
{
    public bool MonitorableLocal
    {
        get => Monitorable;
        set => SetMonitorableDeferred(value);
    }

    public bool MonitoringLocal
    {
        get => Monitoring;
        set => SetMonitoringDeferred(value);
    }

    private bool _monitorableLocal;
    private bool _monitoringLocal;

    public override void _Ready()
    {
        _monitorableLocal = Monitorable;
        _monitoringLocal = Monitoring;
    }

    public void SetMonitoringDeferred(bool value, bool force = false)
    {
        if (value)
        {
            if (_monitoringLocal || force)
            {
                _monitoringLocal = true;
                SetDeferred(Area2D.PropertyName.Monitoring, true);
            }
        }
        else
        {
            if (force)
                _monitoringLocal = false;
            SetDeferred(Area2D.PropertyName.Monitoring, false);
        }
    }

    public void SetMonitorableDeferred(bool value, bool force = false)
    {
        if (value)
        {
            if (_monitorableLocal || force)
            {
                _monitorableLocal = true;
                SetDeferred(Area2D.PropertyName.Monitorable, true);
            }
        }
        else
        {
            if (force)
                _monitorableLocal = false;
            SetDeferred(Area2D.PropertyName.Monitorable, false);
        }
    }
}
