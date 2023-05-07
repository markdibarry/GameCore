using Godot;

namespace GameCore.Utility;

public static class MarginContainerExtensions
{
    /// <summary>
    /// Sets the top margin
    /// </summary>
    /// <param name="container"></param>
    /// <param name="topMargin"></param>
    public static void SetTopMargin(this MarginContainer container, int topMargin)
    {
        container.AddThemeConstantOverride("margin_top", topMargin);
    }

    /// <summary>
    /// Sets the right margin
    /// </summary>
    /// <param name="container"></param>
    /// <param name="rightMargin"></param>
    public static void SetRightMargin(this MarginContainer container, int rightMargin)
    {
        container.AddThemeConstantOverride("margin_right", rightMargin);
    }

    /// <summary>
    /// Sets the bottom margin
    /// </summary>
    /// <param name="container"></param>
    /// <param name="bottomMargin"></param>
    public static void SetBottomMargin(this MarginContainer container, int bottomMargin)
    {
        container.AddThemeConstantOverride("margin_bottom", bottomMargin);
    }

    /// <summary>
    /// Sets the left margin
    /// </summary>
    /// <param name="container"></param>
    /// <param name="leftMargin"></param>
    public static void SetLeftMargin(this MarginContainer container, int leftMargin)
    {
        container.AddThemeConstantOverride("margin_left", leftMargin);
    }

    /// <summary>
    /// Sets margin by one, two, three, or four values.<br/>
    /// When one value is specified, the same margin is applied to all sides.
    /// </summary>
    /// <param name="marginContainer"></param>
    /// <param name="val"></param>
    public static void SetMargin(this MarginContainer marginContainer, int val)
    {
        marginContainer.AddThemeConstantOverride("margin_top", val);
        marginContainer.AddThemeConstantOverride("margin_right", val);
        marginContainer.AddThemeConstantOverride("margin_bottom", val);
        marginContainer.AddThemeConstantOverride("margin_left", val);
    }

    /// <summary>
    /// Sets margin by one, two, three, or four values.<br/>
    /// When two values are specified, the first applies to the top and bottom, the second to the left and right.
    /// </summary>
    /// <param name="marginContainer"></param>
    /// <param name="topBottom"></param>
    /// <param name="leftRight"></param>
    public static void SetMargin(this MarginContainer marginContainer, int topBottom, int leftRight)
    {
        marginContainer.AddThemeConstantOverride("margin_top", topBottom);
        marginContainer.AddThemeConstantOverride("margin_right", leftRight);
        marginContainer.AddThemeConstantOverride("margin_bottom", topBottom);
        marginContainer.AddThemeConstantOverride("margin_left", leftRight);
    }

    /// <summary>
    /// Sets margin by one, two, three, or four values.<br/>
    /// When three values are specified, the first applies to the top, the second to the left and right, and the third to the bottom.
    /// </summary>
    /// <param name="marginContainer"></param>
    /// <param name="top"></param>
    /// <param name="leftRight"></param>
    /// <param name="bottom"></param>
    public static void SetMargin(this MarginContainer marginContainer, int top, int leftRight, int bottom)
    {
        marginContainer.AddThemeConstantOverride("margin_top", top);
        marginContainer.AddThemeConstantOverride("margin_right", leftRight);
        marginContainer.AddThemeConstantOverride("margin_bottom", bottom);
        marginContainer.AddThemeConstantOverride("margin_left", leftRight);
    }

    /// <summary>
    /// Sets margin by one, two, three, or four values.<br/>
    /// When four values are specified, they apply to the top, right, bottom, and left in order.
    /// </summary>
    /// <param name="marginContainer"></param>
    /// <param name="top"></param>
    /// <param name="right"></param>
    /// <param name="bottom"></param>
    /// <param name="left"></param>
    public static void SetMargin(this MarginContainer marginContainer, int top, int right, int bottom, int left)
    {
        marginContainer.AddThemeConstantOverride("margin_top", top);
        marginContainer.AddThemeConstantOverride("margin_right", right);
        marginContainer.AddThemeConstantOverride("margin_bottom", bottom);
        marginContainer.AddThemeConstantOverride("margin_left", left);
    }
}
