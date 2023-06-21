using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameCore.Input;
using Godot;

namespace GameCore.GUI;

public partial class GUIController : CanvasLayer, IGUIController
{
    private readonly List<GUILayer> _guiLayers = new();
    private GUILayer? CurrentLayer => _guiLayers.Count > 0 ? _guiLayers[^1] : null;
    public bool MenuActive { get; private set; }
    public bool DialogActive { get; private set; }
    public bool GUIActive => MenuActive || DialogActive;
    public event Action<GUIController>? GUIStatusChanged;

    /// <summary>
    /// Passes input to current layer for handling.
    /// </summary>
    /// <param name="menuInput"></param>
    /// <param name="delta"></param>
    public void HandleInput(IGUIInputHandler menuInput, double delta)
    {
        CurrentLayer?.HandleInput(menuInput, delta);
    }

    /// <summary>
    /// Closes current layer.
    /// </summary>
    /// <param name="preventAnimation"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public async Task CloseLayerAsync(bool preventAnimation = false, object? data = null)
    {
        GUILayer? layer = CurrentLayer;
        if (layer == null
            || layer.CurrentState == GUILayer.State.Closing
            || layer.CurrentState == GUILayer.State.Closed)
            return;
        await layer.CloseAsync(preventAnimation);
        RemoveChild(layer);
        layer.QueueFree();
        _guiLayers.Remove(layer);
        UpdateCurrentGUI();
        CurrentLayer?.UpdateData(data);
    }

    /// <summary>
    /// Closes all GUI layers
    /// </summary>
    /// <param name="preventAnimation"></param>
    /// <returns></returns>
    public async Task CloseAllLayersAsync(bool preventAnimation = false)
    {
        foreach (var layer in _guiLayers)
        {
            if (!preventAnimation)
                await layer.CloseAsync();
            RemoveChild(layer);
            layer.QueueFree();
        }
        _guiLayers.Clear();
        UpdateCurrentGUI();
    }

    /// <summary>
    /// Opens a dialog session
    /// </summary>
    /// <param name="dialogPath"></param>
    /// <param name="preventAnimation"></param>
    /// <returns></returns>
    public async Task OpenDialogAsync(string dialogPath, bool preventAnimation = false)
    {
        Dialog? dialog = null;
        try
        {
            DialogScript dialogScript = Dialog.LoadScript(dialogPath);
            dialog = new(this, dialogScript);
            AddChild(dialog);
            _guiLayers.Add(dialog);
            UpdateCurrentGUI();
            await dialog.StartDialogAsync();
        }
        catch (Exception ex)
        {
            GD.PrintErr(ex.Message);
            if (dialog == null)
                return;
            await CloseLayerAsync(preventAnimation);
        }
    }

    /// <summary>
    /// Opens a menu layer
    /// </summary>
    /// <param name="scenePath"></param>
    /// <param name="preventAnimation"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public async Task OpenMenuAsync(string scenePath, bool preventAnimation = false, object? data = null)
    {
        await OpenMenuAsync(GD.Load<PackedScene>(scenePath), preventAnimation, data);
    }

    /// <summary>
    /// Opens a menu layer
    /// </summary>
    /// <param name="packedScene"></param>
    /// <param name="preventAnimation"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public async Task OpenMenuAsync(PackedScene packedScene, bool preventAnimation = false, object? data = null)
    {
        Menu? menu = null;
        try
        {
            menu = packedScene.Instantiate<Menu>();
            AddChild(menu);
            _guiLayers.Add(menu);
            UpdateCurrentGUI();
            await menu.InitAsync(this, data);
        }
        catch (Exception ex)
        {
            GD.PrintErr(ex.Message);
            if (menu == null)
                return;
            if (menu is DialogOptionMenu)
                await CloseLayerAsync(preventAnimation);
            await CloseLayerAsync(preventAnimation);
        }
    }

    /// <summary>
    /// Updates current state of GUI
    /// </summary>
    private void UpdateCurrentGUI()
    {
        MenuActive = _guiLayers.Any(x => x is Menu);
        DialogActive = _guiLayers.Any(x => x is Dialog);
        GUIStatusChanged?.Invoke(this);
    }
}
