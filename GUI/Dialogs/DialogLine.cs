using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameCore.Utility;

namespace GameCore.GUI;

public class DialogLine : IStatement, IEventParser
{
    public DialogLine(
        DialogScript script,
        IStorageContext textStorage,
        LineData lineData,
        string translatedText,
        Speaker[] scriptSpeakers,
        bool auto)
    {
        Auto = auto;
        Next = lineData.Next;
        Speakers = scriptSpeakers;
        _dialogScript = script;
        _lineData = lineData;
        _textStorage = textStorage;
        Text = translatedText;
        Speakers = _lineData.SpeakerIndices.Select(x => scriptSpeakers[x]).ToArray();
    }

    private readonly DialogScript _dialogScript;
    private readonly LineData _lineData;
    private readonly IStorageContext _textStorage;
    public bool Auto { get; private set; }
    public GoTo Next { get; private set; }
    public Speaker[] Speakers { get; }
    /// <summary>
    /// Text containing BBCode and Event tags
    /// </summary>
    public string Text { get; }

    public bool SameSpeakers(DialogLine secondLine) => Speaker.SameSpeakers(Speakers, secondLine.Speakers);

    public bool AnySpeakers(DialogLine secondLine) => Speaker.AnySpeakers(Speakers, secondLine.Speakers);

    public bool TryAddTextEvent(StringBuilder sb, List<ITextEvent> events, string tagContent, string fullText, int appendStart, int i, ref int renderedIndex)
    {
        if (!int.TryParse(tagContent, out int intValue))
            return false;

        if (_lineData.InstructionIndices.Length == 0)
            return false;

        // Copied to reduce complexity.
        int renderedIndexCopy = renderedIndex;
        sb.Append(fullText[appendStart..i]);
        ushort[] instructions = _dialogScript.Instructions[_lineData.InstructionIndices[intValue]];

        switch ((OpCode)instructions[0])
        {
            case OpCode.Assign:
            case OpCode.MultAssign:
            case OpCode.DivAssign:
            case OpCode.AddAssign:
            case OpCode.SubAssign:
                AddAsTextEvent();
                break;
            case OpCode.String:
            case OpCode.Float:
            case OpCode.Mult:
            case OpCode.Div:
            case OpCode.Add:
            case OpCode.Sub:
            case OpCode.Var:
            case OpCode.Func:
                AppendResult();
                break;
            case OpCode.NewLine:
                AppendNewLine();
                break;
            case OpCode.Speed:
                AddSpeedEvent();
                break;
            case OpCode.Goto:
                HandleGoTo();
                break;
            case OpCode.Auto:
                HandleAuto();
                break;
            case OpCode.SpeakerSet:
                AddSpeakerSetEvent();
                break;
        };

        renderedIndex = renderedIndexCopy;
        return true;

        void AddAsTextEvent()
        {
            InstructionTextEvent textEvent = new(renderedIndexCopy, instructions);
            events.Add(textEvent);
        }

        void AppendResult()
        {
            string result = string.Empty;
            var enumerator = instructions.GetEnumerator<ushort>();
            VarType returnType = DialogInterpreter.GetReturnType(_dialogScript, _textStorage, enumerator);
            enumerator.MoveNext();

            switch (returnType)
            {
                case VarType.String:
                    result = DialogInterpreter.GetStringInstResult(_dialogScript, _textStorage, enumerator);
                    break;
                case VarType.Float:
                    result = DialogInterpreter.GetFloatInstResult(_dialogScript, _textStorage, enumerator).ToString();
                    break;
                case VarType.Void:
                    AddAsTextEvent();
                    break;
            }

            sb.Append(result);
            renderedIndexCopy += result.Length;
        }

        void HandleAuto() => Auto = instructions[1] == 1;

        void AppendNewLine()
        {
            sb.Append('\r');
            renderedIndexCopy++;
        }

        void AddSpeedEvent()
        {
            SpeedTextEvent textEvent = new(renderedIndexCopy, _dialogScript.InstFloats[instructions[1]]);
            events.Add(textEvent);
        }

        void HandleGoTo() => Next = new GoTo(StatementType.Section, instructions[1]);

        void AddSpeakerSetEvent()
        {
            var enumerator = instructions.GetEnumerator<ushort>();
            SpeakerUpdate speakerUpdate = DialogInterpreter.GetSpeakerUpdate(_dialogScript, enumerator, _textStorage);
            SpeakerTextEvent textEvent = new(
                renderedIndexCopy,
                speakerUpdate.SpeakerId,
                speakerUpdate.DisplayName,
                speakerUpdate.PortraitId,
                speakerUpdate.Mood);
            events.Add(textEvent);
        }
    }
}
