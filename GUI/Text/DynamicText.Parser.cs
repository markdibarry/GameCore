using System.Collections.Generic;
using System.Text;

namespace GameCore.GUI;

public partial class DynamicText
{
    public static string GetEventParsedText(string fullText, string bbCodeParsedText, List<TextEvent> textEvents)
    {
        StringBuilder sb = new();
        int appendStart = 0;
        int renderedIndex = 0;
        int parsedIndex = 0;
        int i = 0;

        while (i < fullText.Length)
        {
            if (fullText[i] != '[' || (i != 0 && fullText[i - 1] == '\\'))
            {
                i++;
                renderedIndex++;
                parsedIndex++;
                continue;
            }

            int bracketLength = GetBracketLength(fullText, i);

            // If doesn't close, ignore
            if (fullText[i + bracketLength - 1] != ']')
            {
                i += bracketLength;
                renderedIndex += bracketLength;
                parsedIndex += bracketLength;
                continue;
            }

            // is bbCode, so only increase fullText index
            if (bbCodeParsedText[parsedIndex] != '[')
            {
                i += bracketLength;
                continue;
            }

            if (!TryAddTextEvent(fullText[(i + 1)..(i + bracketLength)]))
            {
                i += bracketLength;
                renderedIndex += bracketLength;
                parsedIndex += bracketLength;
                continue;
            }

            sb.Append(fullText[appendStart..i]);
            i += bracketLength;
            parsedIndex += bracketLength;
            appendStart = i;
        }
        sb.Append(fullText[appendStart..]);
        return sb.ToString();

        bool TryAddTextEvent(string tagContent)
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
            if (split[0] == "speed")
                return TryAddSpeedEvent(split[1]);
            else if (split[0] == "pause")
                return TryAddPauseEvent(split[1]);
            return false;

            bool TryAddSpeedEvent(string value)
            {
                if (split.Length == 1 && isClosing)
                {
                    textEvents.Add(new SpeedTextEvent(renderedIndex, 1));
                    return true;
                }
                else if (split.Length == 2 && !isClosing)
                {
                    if (!double.TryParse(value, out double parsedValue))
                        return false;
                    textEvents.Add(new SpeedTextEvent(renderedIndex, parsedValue));
                    return true;
                }
                return false;
            }

            bool TryAddPauseEvent(string value)
            {
                if (isClosing)
                    return false;
                if (split.Length == 2)
                {
                    if (!double.TryParse(value, out double parsedValue))
                        return false;
                    textEvents.Add(new PauseTextEvent(renderedIndex, parsedValue));
                    return true;
                }
                return false;
            }
        }
    }

    private static int GetBracketLength(string text, int i)
    {
        int length = 1;
        i++;
        while (i < text.Length)
        {
            if (text[i] == ']' && text[i - 1] != '\\')
                return ++length;
            else if (text[i] == '[' && text[i - 1] != '\\')
                return length;
            length++;
            i++;
        }
        return length;
    }
}
