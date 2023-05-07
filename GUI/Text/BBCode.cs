using System;
using System.Linq;

namespace GameCore.GUI;

public static class BBCode
{
    private static readonly string[] s_bbCodeTags = new[]
    {
        "b",
        "i",
        "u",
        "s",
        "code",
        "p",
        "center",
        "right",
        "left",
        "fill",
        "indent",
        "url",
        "img",
        "font",
        "font_size",
        "opentype_features",
        "table",
        "cell",
        "ul",
        "ol",
        "lb",
        "rb",
        "color",
        "bgcolor",
        "fgcolor",
        "outline_size",
        "outline_color",
        "wave",
        "tornado",
        "fade",
        "rainbow",
        "shake"
    };

    public static bool IsBBCode(string name)
    {
        return s_bbCodeTags.Contains(name);
    }
}
