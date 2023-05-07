using GameCore.Utility;
using Godot;

namespace GameCore.GUI;

public partial class StateDisplay : Control
{
    public static string GetScenePath() => GDEx.GetScenePath();
}
