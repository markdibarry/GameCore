using GameCore.Utility;
using Godot;

namespace GameCore.GUI;

[Tool]
public partial class DialogOptionMenu : Menu
{
    public static string GetScenePath() => GDEx.GetScenePath();
}
