using System;
using System.Threading.Tasks;
using GameCore.Utility;
using Godot;

namespace GameCore.GUI;

[Tool]
public partial class DynamicTextContainer : PanelContainer
{
    private DynamicTextBox _dynamicTextBox = null!;
    private bool _loading;
    [Export]
    public int CurrentPage
    {
        get => _dynamicTextBox.CurrentPage;
        set => _dynamicTextBox.CurrentPage = value;
    }
    [Export(PropertyHint.MultilineText)]
    public string CustomText
    {
        get => _dynamicTextBox.CustomText;
        set => _dynamicTextBox.CustomText = value;
    }
    [Export]
    public bool ShowToEndCharEnabled
    {
        get => _dynamicTextBox.ShowToEndCharEnabled;
        set => _dynamicTextBox.ShowToEndCharEnabled = value;
    }
    [Export]
    public bool WriteTextEnabled
    {
        get => _dynamicTextBox.Writing;
        set => _dynamicTextBox.Writing = value;
    }
    [Export]
    public double SpeedMultiplier
    {
        get => _dynamicTextBox.SpeedMultiplier;
        set => _dynamicTextBox.SpeedMultiplier = value;
    }

    public event Action? StoppedWriting;

    public override void _Ready()
    {
        SetDefault();
    }

    public Task UpdateTextAsync(string text)
    {
        CustomText = text;
        return Task.CompletedTask;
    }

    private void Init()
    {
        SetNodeReferences();
        SubscribeEvents();
    }

    private void OnStoppedWriting()
    {
        StoppedWriting?.Invoke();
    }

    private void SetDefault()
    {
        if (!this.IsSceneRoot())
            return;
        CustomText = "Placeholder Text";
    }

    private void SetNodeReferences()
    {
        _dynamicTextBox = GetNodeOrNull<DynamicTextBox>("DynamicTextBox");
    }

    private void SubscribeEvents()
    {
        _dynamicTextBox.StoppedWriting += OnStoppedWriting;
    }

    private void UnsubscribeEvents()
    {
        _dynamicTextBox.StoppedWriting -= OnStoppedWriting;
    }
}
