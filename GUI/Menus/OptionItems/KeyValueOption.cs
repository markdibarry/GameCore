using GameCore.Utility;
using Godot;

namespace GameCore.GUI;
[Tool]
public partial class KeyValueOption : OptionItem
{
    public static string GetScenePath() => GDEx.GetScenePath();
    private string _keyText = string.Empty;
    private string _valueText = string.Empty;
    [Export(PropertyHint.MultilineText)]
    public string KeyText
    {
        get => _keyText;
        set
        {
            _keyText = value;
            if (KeyLabel != null)
                KeyLabel.Text = _keyText;
        }
    }
    [Export(PropertyHint.MultilineText)]
    public string ValueText
    {
        get => _valueText;
        set
        {
            _valueText = value;
            if (ValueLabel != null)
                ValueLabel.Text = _valueText;
        }
    }
    public Label KeyLabel { get; private set; } = null!;
    public Label ValueLabel { get; private set; } = null!;

    public override void _Ready()
    {
        base._Ready();
        KeyLabel = GetNodeOrNull<Label>("%Key");
        KeyLabel.Text = _keyText;
        ValueLabel = GetNodeOrNull<Label>("%Value");
        ValueLabel.Text = _valueText;
    }
}
