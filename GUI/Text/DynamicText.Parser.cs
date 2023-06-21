using System.Collections.Generic;
using System.Text;

namespace GameCore.GUI;

public partial class DynamicText
{
    public bool TryAddTextEvent(StringBuilder sb, List<ITextEvent> events, string tagContent, string fullText, int appendStart, int i, ref int renderedIndex)
    {
        // currently only supports 'pause' and 'speed'
        if (tagContent.Contains(' '))
            return false;
        bool isClosing = false;
        if (tagContent.StartsWith('/'))
        {
            isClosing = true;
            tagContent = tagContent[1..];
        }

        string[] split = tagContent.Split('=');
        bool result = split[0] switch
        {
            "speed" => TryAddSpeedEvent(split[1], renderedIndex),
            "pause" => TryAddPauseEvent(split[1], renderedIndex),
            _ => false
        };

        if (result)
            sb.Append(fullText[appendStart..i]);
        return result;

        bool TryAddSpeedEvent(string value, int renderedIndex)
        {
            if (split.Length == 1 && isClosing)
            {
                events.Add(new SpeedTextEvent(renderedIndex, 1));
                return true;
            }
            else if (split.Length == 2 && !isClosing)
            {
                if (!double.TryParse(value, out double parsedValue))
                    return false;
                events.Add(new SpeedTextEvent(renderedIndex, parsedValue));
                return true;
            }
            return false;
        }

        bool TryAddPauseEvent(string value, int renderedIndex)
        {
            if (isClosing)
                return false;
            if (split.Length == 2)
            {
                if (!double.TryParse(value, out double parsedValue))
                    return false;
                events.Add(new PauseTextEvent(renderedIndex, parsedValue));
                return true;
            }
            return false;
        }
    }
}
