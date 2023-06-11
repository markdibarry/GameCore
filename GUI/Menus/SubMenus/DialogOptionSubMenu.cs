using System;
using System.Collections.Generic;
using System.Linq;
using GameCore.Utility;
using Godot;

namespace GameCore.GUI;

[Tool]
public partial class DialogOptionSubMenu : OptionSubMenu
{
    public static string GetScenePath() => GDEx.GetScenePath();
    public Choice[] DialogChoices { get; set; } = Array.Empty<Choice>();
    private PackedScene _textOptionScene = GD.Load<PackedScene>(TextOption.GetScenePath());
    private OptionContainer _options = null!;

    protected override void OnPreSetup(object? data)
    {
        if (data is not IEnumerable<Choice> choices)
            return;
        DialogChoices = choices.ToArray();
    }

    protected override void OnItemPressed(OptionContainer optionContainer, OptionItem? optionItem)
    {
        if (optionItem?.OptionData is not int selectedIndex)
            return;
        List<Choice> data = new(1) { DialogChoices[selectedIndex] };
        _ = CloseSubMenuAsync(data: data);
    }

    protected override void OnSetup()
    {
        SetNodeReferences();
        if (DialogChoices.Length == 0)
            return;
        List<TextOption> options = new();
        for (int i = 0; i < DialogChoices.Length; i++)
        {
            if (DialogChoices[i].Disabled)
                continue;
            var textOption = _textOptionScene.Instantiate<TextOption>();
            textOption.LabelText = DialogChoices[i].Text;
            textOption.OptionData = i;
            options.Add(textOption);
        }
        _options.ReplaceChildren(options);
    }

    private void SetNodeReferences()
    {
        _options = GetNode<OptionContainer>("%DialogOptions");
        AddContainer(_options);
    }
}
