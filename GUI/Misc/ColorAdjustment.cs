using System;
using Godot;

namespace GameCore.GUI;

[Tool]
public partial class ColorAdjustment : CanvasLayer
{
    private ShaderMaterial _colorShader = null!;
    [Export(PropertyHint.Range, "-1,1")]
    public float Brightness
    {
        get
        {
            if (_colorShader != null)
                return (float)_colorShader.GetShaderParameter("_brightness");
            return 0;
        }
        set => _colorShader?.SetShaderParameter("_brightness", value);
    }
    [Export(PropertyHint.Range, "-1,1")]
    public float Contrast
    {
        get
        {
            if (_colorShader != null)
                return (float)_colorShader.GetShaderParameter("_contrast");
            return 1;
        }
        set => _colorShader?.SetShaderParameter("_contrast", value);
    }
    /// <summary>
    /// Controls staturation level of the display lower than the current layer.
    /// Range is from -1 for fully desaturated to 2 for more saturated.
    /// </summary>
    [Export(PropertyHint.Range, "-1,2")]
    public float Saturation
    {
        get
        {
            if (_colorShader != null)
                return (float)_colorShader.GetShaderParameter("_saturation");
            return 1;
        }
        set => _colorShader?.SetShaderParameter("_saturation", Math.Clamp(value, -1, 2));
    }
    [Export(PropertyHint.ColorNoAlpha)]
    public Color TintColor
    {
        get
        {
            if (_colorShader != null)
                return (Color)_colorShader.GetShaderParameter("_tint_color");
            return Godot.Colors.White;
        }
        set => _colorShader?.SetShaderParameter("_tint_color", value);
    }
    [Export(PropertyHint.Range, "0,1")]
    public float TintAmount
    {
        get
        {
            if (_colorShader != null)
                return (float)_colorShader.GetShaderParameter("_tint_amount");
            return 0;
        }
        set => _colorShader?.SetShaderParameter("_tint_amount", value);
    }

    public override void _Ready()
    {
        var rect = GetNodeOrNull<ColorRect>("ColorRect");
        _colorShader = (ShaderMaterial)rect.Material;
    }

    public void Reset()
    {
        Brightness = 0;
        Contrast = 0;
        Saturation = 0;
        TintColor = Godot.Colors.White;
        TintAmount = 0;
    }
}
