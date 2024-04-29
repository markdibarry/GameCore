using System.Collections.Generic;
using System.Text;

namespace GameCore.GUI;

public interface IEventParser
{
    bool TryAddTextEvent(StringBuilder sb, List<ITextEvent> events, string tagContent, string fullText, int appendStart, int i, ref int renderedIndex);

    /// <summary>
    /// Takes text with BBCode removed and extracts events along with their character positions
    /// </summary>
    /// <param name="bbCodeParsedText"></param>
    /// <param name="events"></param>
    /// <returns></returns>
    string GetEventParsedText(string fullText, string bbCodeParsedText, List<ITextEvent> events)
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

            // is bbCode, so only increase Text index
            if (bbCodeParsedText[parsedIndex] != '[')
            {
                i += bracketLength;
                continue;
            }

            string tagContent = fullText[(i + 1)..(i + bracketLength - 1)];

            if (!TryAddTextEvent(sb, events, tagContent, fullText, appendStart, i, ref renderedIndex))
            {
                i += bracketLength;
                renderedIndex += bracketLength;
                parsedIndex += bracketLength;
                continue;
            }

            i += bracketLength;
            parsedIndex += bracketLength;
            appendStart = i;
        }

        sb.Append(fullText[appendStart..i]);
        return sb.ToString();
    }

    static int GetBracketLength(string text, int i)
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
