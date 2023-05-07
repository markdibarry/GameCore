using GameCore.Utility;
using Godot;

namespace GameCore.GUI;

[Tool]
public partial class NoDisplayOption : OptionItem
{
    public static string GetScenePath() => GDEx.GetScenePath();
}
